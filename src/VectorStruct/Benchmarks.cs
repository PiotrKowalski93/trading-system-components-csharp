using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;

namespace VectorStruct
{
    [MemoryDiagnoser]
    public class Benchmarks
    {
        [Benchmark]
        public void ScalarSum()
        {
            int sum = 0;
            int[] array = new int[1000000];

            for (int i = 0; i < 1000000; i++)
            {
                sum += i;
            }
        }
        
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<Benchmarks>();
        }
    }
}
