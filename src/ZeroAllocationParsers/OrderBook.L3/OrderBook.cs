using MemoryPool.Lib;
using System.Runtime.InteropServices;

namespace OrderBook.L3
{
    public unsafe sealed class OrderBook : IDisposable
    {
        private const int MAX_ORDERS = 1_500_000;
        private const int MAX_PRICE_LEVELS = 1_000;

        private readonly string _tick;
        // fixed char _tick[16]; // Assuming max tick length is 15 + null terminator

        // MemoryPools
        private MemoryPool<Order> _orderPool;
        private MemoryPool<PriceLevel> _priceLevelPool;

        private PriceLevel* _askHead;
        private PriceLevel* _bidHead;

        private Order** _ordersById;
        private PriceLevel** _priceLevelsByPrice;

        private int OrderIdToIndex(long orderId)
        {
            return (int)(orderId % MAX_ORDERS);
        }
        
        private int PriceToIndex(int price)
        {
            return price % MAX_PRICE_LEVELS;
        }

        public OrderBook(string tick)
        {
            _tick = tick;
            
            _ordersById = (Order**)NativeMemory.Alloc((nuint)MAX_ORDERS, (nuint)sizeof(Order*));
            _priceLevelsByPrice = (PriceLevel**)NativeMemory.Alloc((nuint)MAX_PRICE_LEVELS, (nuint)sizeof(PriceLevel*));

            _orderPool = new MemoryPool<Order>(MAX_ORDERS);
            _priceLevelPool = new MemoryPool<PriceLevel>(MAX_PRICE_LEVELS);
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

        public void Dispose()
        {
            _orderPool.Dispose();
            _priceLevelPool.Dispose();

            NativeMemory.Free(_ordersById);
            NativeMemory.Free(_priceLevelsByPrice);
        }
    }
}
