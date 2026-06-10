using MemoryPool.Lib;
using System.Runtime.InteropServices;

namespace OrderBook.L3
{
    public unsafe sealed class OrderBook : IDisposable
    {
        private const int MAX_ORDERS = 1_500_000;
        private const int MAX_PRICE_LEVELS = 1_000;

        //private readonly string _tick;
        // fixed char _tick[16];

        // MemoryPools
        private MemoryPool<Order> _orderPool;
        private MemoryPool<PriceLevel> _priceLevelPool;

        //private PriceLevel* _askHead;
        //private PriceLevel* _bidHead;
        //private void GetBestBid()
        //{

        //}

        //private void GetBestAsk()
        //{

        //}

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

        public OrderBook()
        {
            //_tick = tick;
            
            _ordersById = (Order**)NativeMemory.Alloc((nuint)MAX_ORDERS, (nuint)sizeof(Order*));
            _priceLevelsByPrice = (PriceLevel**)NativeMemory.Alloc((nuint)MAX_PRICE_LEVELS, (nuint)sizeof(PriceLevel*));

            _orderPool = new MemoryPool<Order>(MAX_ORDERS);
            _priceLevelPool = new MemoryPool<PriceLevel>(MAX_PRICE_LEVELS);
        }

        public void Add(long OrderId, int Side, int Price, int Quantity)
        {
            Order* newOrder = _orderPool.Allocate();
            newOrder->OrderId = OrderId;
            newOrder->Side = Side;
            newOrder->Price = Price;
            newOrder->Quantity = Quantity;
            newOrder->Prev = null;
            newOrder->Next = null;

            int orderIndex = OrderIdToIndex(OrderId);
            _ordersById[orderIndex] = newOrder;

            int priceIndex = PriceToIndex(Price);
            PriceLevel* priceLevel = _priceLevelsByPrice[priceIndex];

            if(priceLevel == null) 
            {
                priceLevel = _priceLevelPool.Allocate();

                priceLevel->Price = Price;
                priceLevel->Side = Side;
                priceLevel->TotalQuantity = 0;
                priceLevel->OrdersCount = 0;
                priceLevel->FirstOrder = null;
                priceLevel->LastOrder = null;

                // Do i need it?
                //priceLevel->Next = null;
                //priceLevel->Prev = null;
            }

            newOrder->PriceLevel = priceLevel;

            if(priceLevel->FirstOrder == null)
            {
                priceLevel->FirstOrder = newOrder;
                priceLevel->LastOrder = newOrder;
            }
            else
            {
                priceLevel->LastOrder->Next = newOrder;
                newOrder->Prev = priceLevel->LastOrder;
                priceLevel->LastOrder = newOrder;
            }
        
            priceLevel->TotalQuantity += Quantity;
            priceLevel->OrdersCount += 1;
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
