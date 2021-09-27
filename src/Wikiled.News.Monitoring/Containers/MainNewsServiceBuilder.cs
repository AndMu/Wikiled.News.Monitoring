using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wikiled.Common.Utilities.Modules;
using Wikiled.News.Monitoring.Feeds;
using Wikiled.News.Monitoring.Monitoring;
using Wikiled.News.Monitoring.Persistency;
using Wikiled.News.Monitoring.Readers;
using Wikiled.News.Monitoring.Readers.Parsers;
using Wikiled.News.Monitoring.Retriever;

namespace Wikiled.News.Monitoring.Containers
{
    public static class MainNewsServiceBuilder
    {
        public static IServiceCollection AddNewsServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<ITrackedRetrieval, TrackedRetrieval>();
            services.AddTransient<IReadingSession, ReadingSession>();
            services.AddTransient<IArticleDataReader, ArticleDataReader>();
            services.AddSingleton<IArticlesMonitor, ArticlesMonitor>();
            services.AddTransient<IFeedsHandler, FeedsHandler>();
            services.AddTransient<IArticlesPersistency, ArticlesPersistency>();
            services.AddSingleton<DefaultPageParser>();

            var configData = new ParsingConfig();
            configuration.GetSection("Scrapper:Parsers").Bind(configData);
            if (configData.Simple != null)
            {
                foreach (var feed in configData.Simple)
                {
                    services.AddSingleton<IPageParser>(ctx => new SimplePageParser(ctx.GetRequiredService<ILogger<SimplePageParser>>(), feed));
                }
            }

            var feedData = new FeedNames();
            configuration.GetSection("Scrapper:RssFeeds").Bind(feedData);
            services.AddSingleton(feedData.Feeds);

            return services;
        }
    }
}
