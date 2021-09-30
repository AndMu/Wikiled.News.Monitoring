using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Wikiled.News.Monitoring.Data;
using Wikiled.News.Monitoring.Readers;

namespace Wikiled.News.Monitoring.Tests.Acceptance
{
    [TestFixture]
    public class ArticleTests
    {
        [Test]
        public async Task ReadArticle()
        {
            var tokenSource = new CancellationTokenSource(10000);
            var articleDefinition = new ArticleDefinition
            {
                Url = new Uri(@"http://www.guardian.co.uk")
            };

            var article = await Global.Services.GetRequiredService<IArticleDataReader>().Read(articleDefinition, tokenSource.Token).ConfigureAwait(false);
            Assert.Greater(article.Content.Text.Length, 100);
        }
    }
}
