using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;

namespace OrderBook.L3.Benchmarks
{
    [MemoryDiagnoser]
    // TODO: Make it work
    //[HardwareCounters(HardwareCounter.BranchMispredictions, HardwareCounter.BranchInstructions)]
    [SimpleJob(warmupCount: 1, iterationCount: 5)]
    public class OrderBookBenchmark
    {
        private List<OrderEvent> _events;
        private OrderBook _book;
        private OrderManager _manager;

        [GlobalSetup]
        public void Setup()
        {
            _events = CsvFeedReader
                .Read("Feed.csv")
                .ToList();

            var ADDs = _events.Where(e => e.Type == EventType.Add).Count();
            var MODIFYs = _events.Where(e => e.Type == EventType.Modify).Count();
            var CANCELs = _events.Where(e => e.Type == EventType.Cancel).Count();
            var EXECUTEs = _events.Where(e => e.Type == EventType.Execute).Count();

            Console.WriteLine("------------------------------------------------------");
            Console.WriteLine($"Loaded {_events.Count} events for benchmarking.");
            Console.WriteLine("------------------------------------------------------");
            Console.WriteLine("ADDs: " + ADDs);
            Console.WriteLine("MODIFYs: " + MODIFYs);
            Console.WriteLine("CANCELs: " + CANCELs);
            Console.WriteLine("EXECUTEs: " + EXECUTEs);
            Console.WriteLine("------------------------------------------------------");
            Console.WriteLine();
        }

        [IterationSetup]
        public void IterationSetup()
        {
            _book = new OrderBook("Sample");
            _manager = new OrderManager(_book);
        }

        [Benchmark]
        public void ReplayFeed()
        {
            foreach (var evt in _events)
            {
                _manager.Process(evt);
            }
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<OrderBookBenchmark>();
        }
    }
}
