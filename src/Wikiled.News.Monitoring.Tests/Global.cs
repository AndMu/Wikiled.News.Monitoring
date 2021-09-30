using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Serilog;
using Wikiled.Common.Utilities.Modules;
using Wikiled.News.Monitoring.Containers;
using Wikiled.News.Monitoring.Feeds;
using Wikiled.News.Monitoring.Retriever;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Wikiled.News.Monitoring.Tests
{
    [SetUpFixture]
    public class Global
    {
        private const string DevEnv = "Development";

        public static ServiceProvider Services { get; private set; }

        public static string Env { get; private set; }

        public static IConfiguration Configuration { get; private set; }

        public static ILogger Logger { get; private set; }

        public static ILoggerFactory LoggerFactory { get; private set; }

        public static ServiceCollection Collection { get; private set; }

        public static ServiceProvider SetupServices()
        {
            TestContext.Progress.WriteLine("SetupServices");
            var serilog = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .WriteTo.NUnitOutput()
                .CreateLogger();

            Collection = new ServiceCollection();
            Collection.AddNewsServices(Configuration);
            Collection.SetDefaultScrappingServices();

            Collection.RegisterModule(
                new NetworkModule(
                    new RetrieveConfiguration
                    {
                        LongDelay = 1000,
                        CallDelay = 0,
                        ShortDelay = 1000,
                        LongRetryCodes = new[] { HttpStatusCode.Forbidden },
                        RetryCodes = new[]
                        {
                            HttpStatusCode.Forbidden,
                            HttpStatusCode.RequestTimeout,      // 408
                            HttpStatusCode.InternalServerError, // 500
                            HttpStatusCode.BadGateway,          // 502
                            HttpStatusCode.ServiceUnavailable,  // 503
                            HttpStatusCode.GatewayTimeout       // 504
                        },
                        MaxConcurrent = 1
                    }));

            Collection.AddLogging(
                builder =>
                {
                    builder.SetMinimumLevel(LogLevel.Trace);
                    builder.AddSerilog(serilog);
                });

            return Collection.BuildServiceProvider();
        }

        [OneTimeSetUp]
        public async Task Setup()
        {
            try
            {
                Env = GetEnvironment(TestContext.CurrentContext.TestDirectory);
                Env ??= DevEnv;

                await TestContext.Progress.WriteLineAsync($"Test Environment: {Env}");
                SetupConfig(TestContext.CurrentContext.TestDirectory);
                Services = SetupServices();
                Logger = Services.GetRequiredService<ILogger<Global>>();
                LoggerFactory = Services.GetRequiredService<ILoggerFactory>();
                Logger.LogInformation("Starting tests on {Env}", Env);
            }
            catch (Exception e)
            {
                await TestContext.Progress.WriteLineAsync($"Setup Failed: {e}");
                throw new Exception("Setup Failed", e);
            }
        }

        [OneTimeTearDown]
        public async Task Clean()
        {
            await Services.DisposeAsync();
        }

        private static void SetupConfig(string path)
        {
            TestContext.Progress.WriteLine($"SetupConfig {path}");
            var builder = new ConfigurationBuilder()
                .SetBasePath(path)
                .AddJsonFile("testsettings.json", false, reloadOnChange: true)
                .AddJsonFile($"testsettings.{Env}.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();
        }

        private static string GetEnvironment(string path)
        {
            path = Path.Combine(path, "Properties");
            if (!Directory.Exists(path))
            {
                return Environment.GetEnvironmentVariable("ENVIRONMENT");
            }

            var builder = new ConfigurationBuilder()
                .SetBasePath(path)
                .AddJsonFile("launchSettings.json", true);

            var configuration = builder.Build();
            var configuredEnv = configuration["profiles:Quant.News.Scrapper.AcceptanceTests:environmentVariables:ENVIRONMENT"];
            return configuredEnv ?? Environment.GetEnvironmentVariable("ENVIRONMENT");
        }
    }
}
