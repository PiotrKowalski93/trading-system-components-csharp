namespace OrderBook.L3
{
    public class OrderManager
    {
        private readonly OrderBook _book;

        public OrderManager(OrderBook book)
        {
            _book = book;
        }

        public void Process(OrderEvent evt)
        {
            switch (evt.Type)
            {
                case EventType.Add:
                    _book.Add(evt.OrderId, evt.IsBuy ? 0 : 1, evt.Price, evt.Quantity);
                    break;

                case EventType.Cancel:
                    _book.Cancel(evt.OrderId);
                    break;

                case EventType.Modify:
                    _book.Modify(evt.OrderId, evt.Quantity);
                    break;

                case EventType.Execute:
                    _book.Execute(evt.OrderId, evt.Quantity);
                    break;
            }
        }
    }
}
