using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;

namespace OrderBook.L3.Benchmarks
{
    [MemoryDiagnoser]
    [HardwareCounters(HardwareCounter.BranchMispredictions, HardwareCounter.BranchInstructions)]
    public class OrderBookBenchmark
    {
        private List<OrderEvent> _events;

        [GlobalSetup]
        public void Setup()
        {
            _events = CsvFeedReader
                .Read("Feed.csv")
                .ToList();
        }

        [Benchmark]
        public void ReplayFeed()
        {
            var book = new OrderBook("Sample");
            var manager = new OrderManager(book);

            foreach (var evt in _events)
            {
                manager.Process(evt);
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
