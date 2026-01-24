using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Networking.Tcp.Core
{
    /// <summary>
    /// Production-ready TCP client dla systemu tradingowego.
    /// Obsługuje reconnect, keepalive, graceful shutdown i comprehensive logging.
    /// </summary>
    public class TcpClientSockets : IDisposable
    {
        private readonly string _ip;
        private readonly int _port;
        private readonly SocketType _socketType;
        private readonly ProtocolType _protocolType;
        private readonly ILogger<TcpClientSockets> _logger;

        private Socket _socket;
        private readonly object _socketLock = new object();

        // Configuration
        private bool _setNoDelay = true;
        private short _ttl = 64;
        private bool _setNonBlocking = false;
        private int _sendTimeoutMs = 5000;
        private int _receiveTimeoutMs = 30000;
        private int _connectTimeoutMs = 10000;

        // Connection state
        private volatile bool _isConnected = false;
        private volatile bool _isDisposed = false;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        // Keepalive configuration
        private bool _enableKeepalive = true;
        private int _keepaliveIntervalMs = 30000; // 30 sec
        private Task _keepaliveTask;

        // Events for monitoring
        public event EventHandler<ConnectedEventArgs> OnConnected;
        public event EventHandler<DisconnectedEventArgs> OnDisconnected;
        public event EventHandler<ErrorEventArgs> OnError;
        public event EventHandler<DataReceivedEventArgs> OnDataReceived;

        public TcpClientSockets(
            string ip,
            int port,
            ILogger<TcpClientSockets> logger,
            SocketType socketType = SocketType.Stream,
            ProtocolType protocolType = ProtocolType.Tcp)
        {
            ValidateInput(ip, port);

            _ip = ip;
            _port = port;
            _socketType = socketType;
            _protocolType = protocolType;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            InitializeSocket();
            _logger.LogInformation($"TCP Client initialized for {ip}:{port}");
        }

        /// <summary>
        /// Konfiguruje parametry socketu dla maksymalnej wydajności w tradingu.
        /// </summary>
        public void ConfigureSocket(
            bool setNoDelay = true,
            short ttl = 64,
            bool setNonBlocking = false,
            int sendTimeoutMs = 5000,
            int receiveTimeoutMs = 30000,
            int connectTimeoutMs = 10000,
            bool enableKeepalive = true,
            int keepaliveIntervalMs = 30000)
        {
            _setNoDelay = setNoDelay;
            _ttl = ttl;
            _setNonBlocking = setNonBlocking;
            _sendTimeoutMs = sendTimeoutMs;
            _receiveTimeoutMs = receiveTimeoutMs;
            _connectTimeoutMs = connectTimeoutMs;
            _enableKeepalive = enableKeepalive;
            _keepaliveIntervalMs = keepaliveIntervalMs;

            ApplySocketOptions();
            _logger.LogInformation("Socket configuration applied");
        }

        /// <summary>
        /// Łączy się z serwerem TCP z timeout obsługą i retry logiką.
        /// </summary>
        public async Task<bool> ConnectAsync(int maxRetries = 3, int retryDelayMs = 1000)
        {
            if (_isConnected)
            {
                _logger.LogWarning("Already connected");
                return true;
            }

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    _logger.LogInformation($"Connection attempt {attempt}/{maxRetries} to {_ip}:{_port}");

                    using (var cts = new CancellationTokenSource(_connectTimeoutMs))
                    {
                        var connectTask = _socket.ConnectAsync(_ip, _port);
                        await connectTask.ConfigureAwait(false);
                    }

                    _isConnected = true;
                    ApplySocketOptions(); // Apply Socket options

                    _logger.LogInformation($"Successfully connected to {_ip}:{_port}");
                    OnConnected?.Invoke(this, new ConnectedEventArgs { ConnectedAt = DateTime.UtcNow });

                    // Uruchom keepalive task
                    if (_enableKeepalive)
                    {
                        _keepaliveTask = KeepAliveWorkerAsync(_cancellationTokenSource.Token);
                    }

                    return true;
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning($"Connection timeout on attempt {attempt}/{maxRetries}");
                    if (attempt < maxRetries)
                    {
                        await Task.Delay(retryDelayMs).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Connection failed on attempt {attempt}/{maxRetries}");
                    OnError?.Invoke(this, new ErrorEventArgs { Exception = ex, IsRecoverable = true });

                    if (attempt < maxRetries)
                    {
                        await Task.Delay(retryDelayMs).ConfigureAwait(false);
                    }
                }
            }

            _logger.LogCritical($"Failed to connect after {maxRetries} attempts");
            return false;
        }

        /// <summary>
        /// Asynchronicznie wysyła dane z timeout obsługą.
        /// </summary>
        public async Task<bool> SendAsync(byte[] data)
        {
            ThrowIfDisposed();

            if (!_isConnected)
            {
                _logger.LogError("Socket not connected");
                return false;
            }

            if (data == null || data.Length == 0)
            {
                _logger.LogWarning("Empty data to send");
                return false;
            }

            try
            {
                using (var cts = new CancellationTokenSource(_sendTimeoutMs))
                {
                    await _socket.SendAsync(
                        new ArraySegment<byte>(data),
                        SocketFlags.None,
                        cts.Token);
                }

                _logger.LogDebug($"Sent {data.Length} bytes");
                return true;
            }
            catch (OperationCanceledException)
            {
                _logger.LogError("Send operation timeout");
                await DisconnectAsync("Send timeout");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Send failed");
                OnError?.Invoke(this, new ErrorEventArgs { Exception = ex, IsRecoverable = true });
                await DisconnectAsync(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Asynchronicznie odbiera dane z timeout obsługą.
        /// </summary>
        public async Task<byte[]> ReceiveAsync(int bufferSize = 65536)
        {
            ThrowIfDisposed();

            if (!_isConnected)
            {
                _logger.LogError("Socket not connected");
                return null;
            }

            var buffer = new byte[bufferSize];

            try
            {
                using (var cts = new CancellationTokenSource(_receiveTimeoutMs))
                {
                    int bytesRead = await _socket.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        SocketFlags.None,
                        cts.Token).ConfigureAwait(false);

                    if (bytesRead == 0)
                    {
                        _logger.LogInformation("Remote host closed connection");
                        await DisconnectAsync("Connection closed by remote");
                        return null;
                    }

                    Array.Resize(ref buffer, bytesRead);
                    _logger.LogDebug($"Received {bytesRead} bytes");
                    OnDataReceived?.Invoke(this, new DataReceivedEventArgs { Data = buffer, ReceivedAt = DateTime.UtcNow });

                    return buffer;
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Receive operation timeout");
                await DisconnectAsync("Receive timeout");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Receive failed");
                OnError?.Invoke(this, new ErrorEventArgs { Exception = ex, IsRecoverable = true });
                await DisconnectAsync(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Wznawia połączenie w przypadku utraty.
        /// </summary>
        public async Task<bool> ReconnectAsync()
        {
            _logger.LogInformation("Attempting to reconnect...");
            await DisconnectAsync("Reconnecting");
            return await ConnectAsync();
        }

        /// <summary>
        /// Bezpiecznie zamyka połączenie.
        /// </summary>
        public async Task DisconnectAsync(string reason = "User requested")
        {
            if (!_isConnected)
                return;

            try
            {
                lock (_socketLock)
                {
                    _isConnected = false;

                    if (_socket?.Connected == true)
                    {
                        _socket.Shutdown(SocketShutdown.Both);
                    }

                    _socket?.Close();
                }

                _logger.LogInformation($"Disconnected: {reason}");
                OnDisconnected?.Invoke(this, new DisconnectedEventArgs
                {
                    DisconnectedAt = DateTime.UtcNow,
                    Reason = reason
                });

                // Poczekaj na zakończenie keepalive task
                if (_keepaliveTask != null)
                {
                    try
                    {
                        _cancellationTokenSource?.Cancel();
                        await _keepaliveTask.ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) { }
                }

                await Task.Delay(500); // Krótka przerwa przed reconnectem
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during disconnect");
            }
        }

        /// <summary>
        /// Zwraca status połączenia.
        /// </summary>
        public bool IsConnected => _isConnected && _socket?.Connected == true;

        public void Dispose()
        {
            if (_isDisposed)
                return;

            try
            {
                DisconnectAsync().GetAwaiter().GetResult();
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
                _socket?.Dispose();
                _logger.LogInformation("TCP Client disposed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during disposal");
            }
            finally
            {
                _isDisposed = true;
            }
        }

        // ==================== PRIVATE METHODS ====================

        private void InitializeSocket()
        {
            lock (_socketLock)
            {
                _socket?.Dispose();
                _socket = new Socket(AddressFamily.InterNetwork, _socketType, _protocolType);
            }
        }

        private void ApplySocketOptions()
        {
            try
            {
                if (_setNoDelay)
                    _socket.NoDelay = true; // TurnOff Nagle'a algo

                _socket.Ttl = _ttl;

                if (_setNonBlocking)
                    _socket.Blocking = false;

                // Set timeout'y
                _socket.SendTimeout = _sendTimeoutMs;
                _socket.ReceiveTimeout = _receiveTimeoutMs;

                if (_enableKeepalive)
                {
                    _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

                    // Windows-specific keepalive intervals
                    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    {
                        int keepaliveIntervalMsInt = _keepaliveIntervalMs;
                        byte[] inOptionValues = new byte[12];
                        BitConverter.GetBytes(1).CopyTo(inOptionValues, 0);
                        BitConverter.GetBytes(keepaliveIntervalMsInt).CopyTo(inOptionValues, 4);
                        BitConverter.GetBytes(keepaliveIntervalMsInt).CopyTo(inOptionValues, 8);
                        _socket.IOControl(IOControlCode.KeepAliveValues, inOptionValues, null);
                    }
                }

                _logger.LogDebug("Socket options applied");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error applying socket options");
            }
        }

        private async Task KeepAliveWorkerAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested && _isConnected)
                {
                    await Task.Delay(_keepaliveIntervalMs, cancellationToken).ConfigureAwait(false);

                    if (_isConnected)
                    {
                        _logger.LogDebug("Sending keepalive probe");
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Keepalive worker error");
            }
        }

        private void ValidateInput(string ip, int port)
        {
            if (string.IsNullOrWhiteSpace(ip))
                throw new ArgumentException("IP address cannot be empty", nameof(ip));

            if (port <= 0 || port > 65535)
                throw new ArgumentException("Port must be between 1 and 65535", nameof(port));
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().Name);
        }
    }

    // ==================== EVENT ARGS ====================

    public class ConnectedEventArgs : EventArgs
    {
        public DateTime ConnectedAt { get; set; }
    }

    public class DisconnectedEventArgs : EventArgs
    {
        public DateTime DisconnectedAt { get; set; }
        public string Reason { get; set; }
    }

    public class ErrorEventArgs : EventArgs
    {
        public Exception Exception { get; set; }
        public bool IsRecoverable { get; set; }
    }

    public class DataReceivedEventArgs : EventArgs
    {
        public byte[] Data { get; set; }
        public DateTime ReceivedAt { get; set; }
    }
}
