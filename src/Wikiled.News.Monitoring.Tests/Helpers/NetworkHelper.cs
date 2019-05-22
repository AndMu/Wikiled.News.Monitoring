using System.Net;
using Autofac;
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
            var builder = new ContainerBuilder();
            builder.RegisterModule<LoggingModule>();
            builder.RegisterModule<MainNewsModule>();
            builder.RegisterModule<NullNewsModule>();
            builder.RegisterType<SimpleArticleTextReader>().As<IArticleTextReader>();
            builder.RegisterModule(new NewsRetrieverModule(RetrieveConfiguration.GenerateCommon()));

            Container = builder.Build();
            Retrieval = Container.Resolve<ITrackedRetrieval>();
        }

        public IContainer Container { get; }

        public ITrackedRetrieval Retrieval { get; }
    }
}
