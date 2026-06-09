namespace OrderBook.L3
{
    public unsafe struct Order
    {
        public long OrderId;
        public int Side;       // 0 for buy, 1 for sell
        public int Price;
        public int Quantity;

        public Order* Prev;
        public Order* Next;
        
        public PriceLevel* PriceLevel;
    }
}
