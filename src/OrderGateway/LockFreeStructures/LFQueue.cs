using System.Runtime;

namespace LockFreeStructures
{
    internal class LFQueue<T>
    {
        private readonly T[] _buffer;
        private readonly int _mask;

        // tylko producer zapisuje
        private int _writeIndex;

        // tylko consumer zapisuje
        private int _readIndex;

        public int Capacity => _buffer.Length;

        public bool IsEmpty =>
            Volatile.Read(ref _writeIndex) == Volatile.Read(ref _readIndex);

        public bool IsFull =>
            ((Volatile.Read(ref _writeIndex) + 1) & _mask) == Volatile.Read(ref _readIndex);

        public LFQueue(int capacityPowerOfTwo)
        {
            //GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
            if (capacityPowerOfTwo <= 0 ||
                (capacityPowerOfTwo & (capacityPowerOfTwo - 1)) != 0)
            {
                throw new ArgumentException("Capacity must be power of two");
            }

            _buffer = new T[capacityPowerOfTwo];
            _mask = capacityPowerOfTwo - 1;
        }

        // PRODUCER ONLY
        public bool Enqueue(in T item)
        {
            var write = _writeIndex;
            var next = (write + 1) & _mask;

            if (next == Volatile.Read(ref _readIndex))
                return false; // full

            _buffer[write] = item;

            // publish write
            Volatile.Write(ref _writeIndex, next);
            return true;
        }

        // CONSUMER ONLY
        public bool TryDequeue(out T item)
        {
            var read = _readIndex;

            if (read == Volatile.Read(ref _writeIndex))
            {
                item = default!;
                return false; // empty
            }

            item = _buffer[read];
            _buffer[read] = default!; // allow GC

            Volatile.Write(ref _readIndex, (read + 1) & _mask);
            return true;
        }
    }
}
