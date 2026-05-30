namespace OrderBook.L3
{
    public class OrderBook
    {
        private string _tick;

        private Dictionary<long, Order> _orders;

        private Dictionary<int, PriceLevel> _bids;
        private Dictionary<int, PriceLevel> _asks;

        //TODO: Add bestBid and bestAsk fields to optimize retrieval of best prices

        public OrderBook(string tick)
        {
            _tick = tick;
            _orders = new Dictionary<long, Order>(1_500_000);

            _bids = new Dictionary<int, PriceLevel>(1_000_000);
            _asks = new Dictionary<int, PriceLevel>(1_000_000);
        }

        public void Add(Order order)
        {
            _orders[order.OrderId] = order;

            var priceLevel = order.Side == 0 ? _bids : _asks;

            if (!priceLevel.TryGetValue(order.Price, out var level))
            {
                level = new PriceLevel { Price = order.Price };
                priceLevel[order.Price] = level;
            }

            level.TotalQuantity += order.Quantity;
            level.OrdersCount++;
            level._Orders[order.OrderId] = order;
        }

        public void Cancel(long orderId)
        {
            if (_orders.TryGetValue(orderId, out var order))
            {
                var priceLevel = order.Side == 0 ? _bids : _asks;

                if (priceLevel.TryGetValue(order.Price, out var level))
                {
                    level.TotalQuantity -= order.Quantity;
                    level.OrdersCount--;
                    level._Orders.Remove(orderId);

                    if (level.OrdersCount == 0)
                    {
                        priceLevel.Remove(order.Price);
                    }
                }

                _orders.Remove(orderId);
            }
        }

        public void Modify(long orderId, int newQuantity)
        {
            if (_orders.TryGetValue(orderId, out var order))
            {
                var priceLevel = order.Side == 0 ? _bids : _asks;

                if (priceLevel.TryGetValue(order.Price, out var level))
                {
                    level.TotalQuantity += newQuantity - order.Quantity;
                    order.Quantity = newQuantity;
                }
            }
        }

        public void Execute(long orderId, int executedQuantity)
        {
            if (_orders.TryGetValue(orderId, out var order))
            {
                var priceLevel = order.Side == 0 ? _bids : _asks;

                if (priceLevel.TryGetValue(order.Price, out var level))
                {
                    level.TotalQuantity -= executedQuantity;
                    order.Quantity -= executedQuantity;

                    if (order.Quantity <= 0)
                    {
                        level.OrdersCount--;
                        level._Orders.Remove(orderId);
                        _orders.Remove(orderId);

                        if (level.OrdersCount == 0)
                        {
                            priceLevel.Remove(order.Price);
                        }
                    }
                }
            }
        }

        public void Clear()
        {
            _orders.Clear();
            _bids.Clear();
            _asks.Clear();
        }
    }

    public struct Order
    {
        public long OrderId { get; set; }
        public int Side { get; set; }       // 0 for buy, 1 for sell
        public int Price { get; set; }
        public int Quantity { get; set; }
    }

    public struct PriceLevel
    {
        public PriceLevel()
        {
            _Orders = new Dictionary<long, Order>();
        }

        public int Price { get; set; }
        public int TotalQuantity { get; set; }
        public int OrdersCount { get; set; }

        public Dictionary<long, Order> _Orders { get; set; }
    }
}
