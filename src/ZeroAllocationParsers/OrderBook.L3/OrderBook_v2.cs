namespace OrderBook.L3
{
    public struct OrderBook
    {
        private string _tick;

        private int MAX_ORDERS = 1_500_000;
        private int MAX_PRICE_LEVELS = 1_000;

        private Order[] _orders;

        // Sorted by price
        private PriceLevel[] _priceLevels;

        // free indexes for orders
        int[] _freeIndexes;
        int _freeTop;

        public OrderBook(string tick)
        {
            _tick = tick;
            _orders = new Order[MAX_ORDERS];
            _priceLevels = new PriceLevel[MAX_PRICE_LEVELS];
        }

        private int AllocateOrder()
        {
            if (_freeTop > 0)
                return _freeIndexes[--_freeTop];

            return _next++;
        }

        private void Free(int index)
        {
            _freeIndexes[_freeTop++] = index;
        }

        public void Add(long OrderId, int Side, int Price, int Quantity)
        {

        }

        public void Cancel(long orderId)
        {
        }

        public void Modify(long orderId, int newQuantity)
        {
        }

        public void Execute(long orderId, int executedQuantity)
        {
        }
    }

    public struct Order
    {
        public long OrderId { get; set; }
        public int Side { get; set; }       // 0 for buy, 1 for sell
        public int Price { get; set; }
        public int Quantity { get; set; }

        public int prev_ = -1;
        public int next_ = -1;

        public Order()
        {
        }
    }

    public struct PriceLevel
    {
        public int Price { get; set; }
        public int Side { get; set; }       // 0 for buy, 1 for sell
        public int TotalQuantity { get; set; }
        public int OrdersCount { get; set; }

        public int headOrder_ = -1;
        public int tailOrder_ = -1;

        public PriceLevel()
        {
        }
    }
}
