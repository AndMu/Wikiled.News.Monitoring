using System.Net;
using Autofac;
using Wikiled.News.Monitoring.Containers;
using Wikiled.News.Monitoring.Readers;
using Wikiled.News.Monitoring.Retriever;

namespace Wikiled.News.Monitoring.Tests.Helpers
{
    public class NetworkHelper
    {
        public NetworkHelper()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<MainModule>();
            builder.RegisterModule<NullModule>();
            builder.RegisterType<SimpleArticleTextReader>().As<IArticleTextReader>();
            builder.RegisterModule(
                new RetrieverModule(new RetrieveConfiguration
                {
                    LongRetryDelay = 1000,
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

            Container = builder.Build();
            Retrieval = Container.Resolve<ITrackedRetrieval>();
        }

        public IContainer Container { get; }

        public ITrackedRetrieval Retrieval { get; }
    }
}
