using Luger.LoggerProvider;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Luger.Playground
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var provider = new LugerLogProvider(new LugerLogOptions
            {
                LugerUrl = new Uri("http://localhost:7931"),
                Bucket = "bucket",
                BatchPostInterval = TimeSpan.FromSeconds(1)
            });

            ILogger logger = provider.CreateLogger("Test category");
            var rng = new Random();
            for (int i = 0; i < 1; i++)
            {
                try
                {
                    throw new InvalidOperationException("Some exception occured level 0", new ArithmeticException("Inner ex level 1", new FormatException("Inner ex level 2")));
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "This is an error with exception! {Label}", 1);
                }
                Console.WriteLine("write");
                // logger.Log((LogLevel)rng.Next(5), "This is test message! With label value={LabelValue}", i);
                
                await Task.Delay(5000);
            }

            provider.Dispose();
        }
    }
}
