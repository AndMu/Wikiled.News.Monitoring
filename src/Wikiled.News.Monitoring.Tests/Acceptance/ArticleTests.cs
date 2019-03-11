using Autofac;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;
using Wikiled.News.Monitoring.Data;
using Wikiled.News.Monitoring.Readers;
using Wikiled.News.Monitoring.Tests.Helpers;

namespace Wikiled.News.Monitoring.Tests.Acceptance
{
    [TestFixture]
    public class ArticleTests
    {
        private NetworkHelper instance;

        [SetUp]
        public void Setup()
        {
            instance = new NetworkHelper();
        }

        [Test]
        public async Task ReadArticle()
        {
            var tokenSource = new CancellationTokenSource(10000);
            var articleDefinition = new ArticleDefinition
            {
                Url = new Uri(@"http://www.guardian.co.uk")
            };

            var article = await instance.Container.Resolve<IArticleDataReader>().Read(articleDefinition, tokenSource.Token).ConfigureAwait(false);
            Assert.Greater(article.Content.Text.Length, 100);
        }
    }
}
