using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Wikiled.News.Monitoring.Feeds;

namespace Wikiled.News.Monitoring.Tests.Acceptance
{
    [TestFixture]
    public class FeedTests
    {
        [Test]
        public async Task GetArticles()
        {
            var feed = Global.Services.GetRequiredService<IFeedsHandler>();
            var data = await feed.GetArticles().ToArray();
            Assert.Greater(data.Length, 1);
            Assert.IsNotEmpty(data[0].Id);
        }
    }
}
