using System;
using System.Collections.Generic;
using System.Text;

namespace OrderBook.L3
{
    public sealed class OrderManager
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

                    var order = new Order
                    {
                        OrderId = evt.OrderId,
                        Side = evt.IsBuy ? 0 : 1,
                        Price = evt.Price,
                        Quantity = evt.Quantity
                    };

                    _book.Add(order);
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

    public enum EventType
    {
        Add,
        Cancel,
        Modify,
        Execute
    }

    public readonly struct OrderEvent
    {
        public EventType Type { get; init; }

        public long OrderId { get; init; }

        public bool IsBuy { get; init; }

        public int Price { get; init; }

        public int Quantity { get; init; }
    }
}
