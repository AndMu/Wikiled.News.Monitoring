using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Wikiled.News.Monitoring.Monitoring;

namespace Wikiled.News.Monitoring.Tests.Acceptance
{
    [TestFixture]
    public class ArticlesMonitoringTests
    {
        [Test]
        public async Task Monitor()
        {
            var monitor = Global.Services.GetRequiredService<IArticlesMonitor>();
            var articles = await monitor.GetCurrentArticles().ToArray();
            Assert.Greater(articles.Length, 1);
        }
    }
}
