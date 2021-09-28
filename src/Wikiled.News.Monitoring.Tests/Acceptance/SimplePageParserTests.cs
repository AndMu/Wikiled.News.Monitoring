using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Wikiled.News.Monitoring.Config;
using Wikiled.News.Monitoring.Data;
using Wikiled.News.Monitoring.Extensions;
using Wikiled.News.Monitoring.Readers.Parsers;

namespace Wikiled.News.Monitoring.Tests.Acceptance
{
    [TestFixture]
    public class SimplePageParserTests
    {
        [Test]
        public async Task Interfax()
        {
            var parser = new SimplePageParser(
                new NullLogger<SimplePageParser>(),
                new FeedParsingConfig
                {
                    ContentPath = "article",
                    TitlePath = "h1[itemprop='headline']"
                });

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://www.interfax.ru/business/793954");
            var byteArray = await client.GetByteArrayAsync(string.Empty);
            var responseString = Encoding.GetEncoding("windows-1251").GetString(byteArray, 0, byteArray.Length);
            var page = responseString.GetDocument();
            var result = parser.Parse(new ArticleDefinition(), page);
            Assert.IsNotEmpty(result.Text);
        }
    }
}
