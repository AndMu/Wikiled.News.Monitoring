using Microsoft.Extensions.DependencyInjection;
using Wikiled.Common.Utilities.Modules;
using Wikiled.News.Monitoring.Readers;

namespace Wikiled.News.Monitoring.Containers
{
    public class NullNewsModule : IModule
    {
        public IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IAuthentication, NullAuthentication>();
            services.AddSingleton<ICommentsReader, NullCommentsReader>();
            services.AddSingleton<IArticleTextReader, SimpleArticleTextReader>();
            return services;
        }
    }
}
