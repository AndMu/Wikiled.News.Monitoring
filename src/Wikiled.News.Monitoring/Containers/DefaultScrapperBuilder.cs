using Microsoft.Extensions.DependencyInjection;
using Wikiled.News.Monitoring.Persistency;
using Wikiled.News.Monitoring.Readers;

namespace Wikiled.News.Monitoring.Containers
{
    public static class DefaultScrapperBuilder
    {
        public static IServiceCollection SetDefaultScrappingServices(this IServiceCollection services)
        {
            services.AddSingleton<IDefinitionTransformer, NullDefinitionTransformer>();
            services.AddSingleton<IAuthentication, NullAuthentication>();
            services.AddSingleton<ICommentsReader, NullCommentsReader>();
            services.AddSingleton<IArticleTextReader, SimpleArticleTextReader>();
            services.AddSingleton<IArticlesPersistency, ArticlesPersistency>();
            return services;
        }
    }
}
