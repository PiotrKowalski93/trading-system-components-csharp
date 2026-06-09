namespace OrderBook.L3
{
    public unsafe struct PriceLevel
    {
        public int Price;
        public int Side;       // 0 for buy, 1 for sell
        public int TotalQuantity;
        public int OrdersCount;

        public Order* FirstOrder;
        public Order* LastOrder;

        public PriceLevel* Prev;
        public PriceLevel* Next;
    }
}
