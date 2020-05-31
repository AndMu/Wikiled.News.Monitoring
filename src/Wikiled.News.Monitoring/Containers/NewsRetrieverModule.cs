using System;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wikiled.Common.Net.Resilience;
using Wikiled.Common.Utilities.Modules;
using Wikiled.News.Monitoring.Retriever;

namespace Wikiled.News.Monitoring.Containers
{
    public class NewsRetrieverModule : IModule
    {
        private readonly RetrieveConfiguration configuration;

        public NewsRetrieverModule(RetrieveConfiguration configuration)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(configuration).As<IResilienceConfig, RetrieveConfiguration>();
            services.AddSingleton(IPAddress.Any);
            services.AddSingleton<IResilience, CommonResilience>();
            services.AddTransient(
                ctx => (Func<Uri, IDataRetriever>)(uri => new SimpleDataRetriever(ctx.GetRequiredService<ILogger<SimpleDataRetriever>>(), ctx.GetRequiredService<IIPHandler>(), uri)));
            services.AddSingleton<IIPHandler, IPHandler>();
            return services;
        }
    }
}
