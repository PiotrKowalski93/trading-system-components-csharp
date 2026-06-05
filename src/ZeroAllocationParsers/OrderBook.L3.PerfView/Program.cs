using OrderBook.L3;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("START");
        Console.ReadLine();

        List<OrderEvent> _events;
        OrderBook.L3.OrderBook_v1 _book;
        OrderManager_v1 _manager;

        _events = CsvFeedReader
                    .Read("Feed.csv")
                    .ToList();

        // Break to run profiler before processing events
        Console.WriteLine("READY");
        Console.ReadLine();

        _book = new OrderBook.L3.OrderBook_v1("Sample");
        _manager = new OrderManager_v1(_book);

        foreach (var evt in _events)
        {
            _manager.Process(evt);
        }

        Console.WriteLine("END");
        Console.ReadLine();
    }
}