using Microsoft.Extensions.DependencyInjection;
using Wikiled.Common.Utilities.Modules;
using Wikiled.News.Monitoring.Feeds;
using Wikiled.News.Monitoring.Monitoring;
using Wikiled.News.Monitoring.Persistency;
using Wikiled.News.Monitoring.Readers;
using Wikiled.News.Monitoring.Retriever;

namespace Wikiled.News.Monitoring.Containers
{
    public class MainNewsModule : IModule
    {
        public IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<ITrackedRetrieval, TrackedRetrieval>();
            services.AddTransient<IReadingSession, ReadingSession>();
            services.AddTransient<IArticleDataReader, ArticleDataReader>();
            services.AddSingleton<IArticlesMonitor, ArticlesMonitor>();
            services.AddTransient<IFeedsHandler, FeedsHandler>();
            services.AddTransient<IArticlesPersistency, ArticlesPersistency>();
            return services;
        }
    }
}
