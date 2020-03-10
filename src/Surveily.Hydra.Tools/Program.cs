// <copyright file="Program.cs" company="Surveily sp. z o.o.">
// Copyright (c) Surveily sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Hydra.Tools.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Hydra.Tools
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var result = new TaskCompletionSource<int>();

            using (var host = CreateHostBuilder(args, result))
            {
                var loggerFactory = host.Services.GetService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("Program");

                try
                {
                    await host.StartAsync();

                    await result.Task;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, ex.Message);
                }
            }
        }

        private static IHost CreateHostBuilder(string[] args, TaskCompletionSource<int> result)
        {
            var host = Host.CreateDefaultBuilder(args)
                           .ConfigureLogging((builder, logging) =>
                           {
                               logging.AddConsole();
                           })
                           .ConfigureAppConfiguration((hostingContext, config) =>
                           {
                               config.SetBasePath(new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName);
                               config.AddEnvironmentVariables();
                           })
                           .ConfigureServices((hostContext, services) =>
                           {
                               services.AddSingleton(args);
                               services.AddSingleton(result);

                               services.AddHostedService<CommandService>();
                               services.AddSingleton<ICommand, CopyCommand>();
                               services.AddSingleton<IConfiguration>(hostContext.Configuration);
                           });

            return host.Build();
        }
    }
}
