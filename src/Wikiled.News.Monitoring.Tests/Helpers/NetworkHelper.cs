using System;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wikiled.Common.Utilities.Modules;
using Wikiled.News.Monitoring.Containers;
using Wikiled.News.Monitoring.Readers;
using Wikiled.News.Monitoring.Retriever;

namespace Wikiled.News.Monitoring.Tests.Helpers
{
    public class NetworkHelper
    {
        public NetworkHelper()
        {
            ServiceCollection builder = new ServiceCollection();
            builder.RegisterModule<LoggingModule>();
            builder.RegisterModule<MainNewsModule>();
            builder.RegisterModule<NullNewsModule>();
            builder.AddSingleton(
                ctx => (Func<ITrackedRetrieval, IArticleTextReader>)(arg => new SimpleArticleTextReader(ctx.GetRequiredService<ILogger<SimpleArticleTextReader>>(), arg)));
            builder.RegisterModule(
                new NewsRetrieverModule(new RetrieveConfiguration
                {
                    LongDelay = 1000,
                    ShortDelay = 100,
                    CallDelay = 50,
                    LongRetryCodes = new[] { HttpStatusCode.Forbidden },
                    RetryCodes = new[]
                    {
                        HttpStatusCode.Forbidden,
                        HttpStatusCode.RequestTimeout, // 408
                        HttpStatusCode.InternalServerError, // 500
                        HttpStatusCode.BadGateway, // 502
                        HttpStatusCode.ServiceUnavailable, // 503
                        HttpStatusCode.GatewayTimeout // 504
                    },
                    MaxConcurrent = 1
                }));

            Container = builder.BuildServiceProvider();
            Retrieval = Container.GetRequiredService<ITrackedRetrieval>();
        }

        public IServiceProvider Container { get; }

        public ITrackedRetrieval Retrieval { get; }
    }
}
