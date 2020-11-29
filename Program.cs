using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using console = System.Console;
using CynetTcpListener.Business;
using CynetTcpListener.Business.Models;
using System.Threading;

namespace CynetTcpListener.Console
{
    public class Program
    {
        public static IConfigurationRoot configuration;
        private static readonly Listener listener;
        private static readonly FileProcessor processor;
        private static readonly ILogger logger;

        static Program()
        {
            var serviceProvider = new ServiceCollection()
                    .AddLogging(cfg => cfg.AddConsole())
                    .Configure<LoggerFilterOptions>(cfg => cfg.MinLevel = LogLevel.Debug)
                    .BuildServiceProvider();
            logger = serviceProvider.GetService<ILogger<Program>>();
            var config = new ConfigurationBuilder()
                  .AddJsonFile("appsettings.json", true, true)
                  .Build();

            var sharedQueue = new ConcurrentQueue<GeneralResponce>();
            processor = new FileProcessor(logger, config["filename"], sharedQueue);
            listener = new Listener(logger, config["ip"], config["port"], sharedQueue);
        }

        public static void Main(string[] args)
        {
            console.WriteLine("TCP LISTENER CONSOLE\nPress 's' key to start TcpListener and FileProcessor" +
                "\nPress 'e' key to stop\nPress 'd' for statistics\nPress 'ESC' for exit");

            do
            {
                switch (console.ReadKey().Key)
                {
                    case ConsoleKey.S:
                        logger.LogInformation("Running...");
                        listener.Run();
                        processor.Run();
                        logger.LogInformation("Runned");
                        break;
                    case ConsoleKey.E:
                        logger.LogInformation("Stopping...");
                        listener.Stop();
                        processor.Stop();
                        logger.LogInformation("Stopped");
                        break;
                    case ConsoleKey.D:
                        PrintThreadStatistic();
                        break;
                    default:
                        break;
                }
            } while (console.ReadKey(true).Key != ConsoleKey.Escape);
        }

        private static void PrintThreadStatistic()
        {
            ThreadPool.GetAvailableThreads(out var available, out var _);
            ThreadPool.GetMaxThreads(out var maxThreads, out var _);
            ThreadPool.GetMinThreads(out var minThreads, out var _);

            console.WriteLine($"Active threads: {maxThreads - available}");
        }
    }
}
