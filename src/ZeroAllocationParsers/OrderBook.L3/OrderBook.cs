namespace OrderBook.L3
{
    public class OrderBook
    {
        private string _tick;

        private SortedDictionary<long, Order> _orders;

        private Dictionary<decimal, PriceLevel> _bids;
        private Dictionary<decimal, PriceLevel> _asks;

        //TODO: Add bestBid and bestAsk fields to optimize retrieval of best prices

        public OrderBook(string tick)
        {
            _tick = tick;
            _orders = new SortedDictionary<long, Order>();

            _bids = new Dictionary<decimal, PriceLevel>();
            _asks = new Dictionary<decimal, PriceLevel>();
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
    }

    public struct Order
    {
        public long OrderId { get; set; }
        public int Side { get; set; }       // 0 for buy, 1 for sell
        public int Price { get; set; }
        public int Quantity { get; set; }
    }

    public class PriceLevel
    {
        public int Price { get; set; }
        public int TotalQuantity { get; set; }
        public int OrdersCount { get; set; }

        public SortedDictionary<long, Order> _Orders { get; set; } = new SortedDictionary<long, Order>();
    }
}
