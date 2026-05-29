using System;
using System.Collections.Generic;
using System.Text;

namespace OrderBook.L3
{
    public static class CsvFeedReader
    {
        public static IEnumerable<OrderEvent> Read(string path)
        {
            using var reader = new StreamReader(path);

            reader.ReadLine(); // header

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();

                var parts = line.Split(',');

                yield return new OrderEvent
                {
                    Type = ParseType(parts[1]),
                    OrderId = long.Parse(parts[2]),
                    IsBuy = parts[3] == "BUY",
                    Price = int.Parse(parts[4]),
                    Quantity = int.Parse(parts[5])
                };
            }
        }

        private static EventType ParseType(string type)
        {
            return type switch
            {
                "ADD" => EventType.Add,
                "CANCEL" => EventType.Cancel,
                "MODIFY" => EventType.Modify,
                "EXECUTE" => EventType.Execute,
                _ => throw new ArgumentException($"Unknown event type: {type}")
            };
        }
    }
}
