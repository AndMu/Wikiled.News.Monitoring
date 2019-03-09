using System.Threading;
using Autofac;
using Wikiled.Common.Logging;
using Wikiled.Common.Utilities.Modules;
using Wikiled.News.Monitoring.Feeds;
using Wikiled.News.Monitoring.Monitoring;
using Wikiled.News.Monitoring.Persistency;
using Wikiled.News.Monitoring.Readers;
using Wikiled.News.Monitoring.Retriever;

namespace Wikiled.News.Monitoring.Containers
{
    public class MainModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule(new LoggingModule(ApplicationLogging.LoggerFactory));
            builder.RegisterType<TrackedRetrieval>().As<ITrackedRetrieval>();
            builder.RegisterType<ReadingSession>().As<IReadingSession>().OnActivating(async item => await item.Instance.Initialize(CancellationToken.None).ConfigureAwait(false));
            builder.RegisterType<ArticleDataReader>().As<IArticleDataReader>();
            builder.RegisterType<ArticlesMonitor>().As<IArticlesMonitor>().SingleInstance();
            builder.RegisterType<FeedsHandler>().As<IFeedsHandler>();
            builder.RegisterType<ArticlesPersistency>().As<IArticlesPersistency>();
        }
    }
}
