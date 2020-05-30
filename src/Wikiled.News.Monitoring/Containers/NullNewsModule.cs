using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wikiled.Common.Utilities.Modules;
using Wikiled.News.Monitoring.Data;
using Wikiled.News.Monitoring.Readers;
using Wikiled.News.Monitoring.Retriever;

namespace Wikiled.News.Monitoring.Containers
{
    public class NullNewsModule : IModule
    {

        public IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(
                ctx => (Func<ITrackedRetrieval, IAuthentication>)(arg => new NullAuthentication(arg)));
            services.AddSingleton(
                ctx => (Func<ITrackedRetrieval, IArticleTextReader>)(arg => new SimpleArticleTextReader(ctx.GetRequiredService<ILogger<SimpleArticleTextReader>>(), arg)));
            services.AddSingleton(
                ctx => (Func<ITrackedRetrieval, ArticleDefinition, ICommentsReader>)((arg, def) => new NullCommentsReader()));
            services.AddSingleton<ICommentsReader, NullCommentsReader>();
            return services;
        }
    }
}
