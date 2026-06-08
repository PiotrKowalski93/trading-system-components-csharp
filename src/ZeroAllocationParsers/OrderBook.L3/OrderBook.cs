namespace OrderBook.L3
{
    public unsafe struct OrderBook
    {
        private const int MAX_ORDERS = 1_500_000;
        private const int MAX_PRICE_LEVELS = 1_000;

        private string _tick;

        private PriceLevel* ask_PriceLevels_Head_;
        private PriceLevel* bid_PriceLevels_Head_;

        private Order*[] orders_;
        private PriceLevel*[] priceLevels_;

        private long OrderIdToIndex(long orderId)
        {
            return orderId % MAX_ORDERS;
        }
        
        private int PriceToIndex(int price)
        {
            return price % MAX_PRICE_LEVELS;
        }

        public OrderBook(string tick)
        {
            _tick = tick;
            orders_ = new Order*[MAX_ORDERS];
            priceLevels_ = new PriceLevel*[MAX_PRICE_LEVELS];
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

    public unsafe struct Order
    {
        public long OrderId { get; set; }
        public int Side { get; set; }       // 0 for buy, 1 for sell
        public int Price { get; set; }
        public int Quantity { get; set; }

        public Order* prev_;
        public Order* next_;
        
        public PriceLevel* PriceLevel;
    }

    public unsafe struct PriceLevel
    {
        public int Price { get; set; }
        public int Side { get; set; }       // 0 for buy, 1 for sell
        public int TotalQuantity { get; set; }
        public int OrdersCount { get; set; }

        public Order* FirstOrder;
        public Order* LastOrder;

        public PriceLevel* PriceLevelPrev;
        public PriceLevel* PriceLevelNext;
    }
}
