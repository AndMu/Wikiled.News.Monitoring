using NUnit.Framework;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Wikiled.News.Monitoring.Retriever;

namespace Wikiled.News.Monitoring.Tests.Acceptance
{
    [TestFixture]
    public class TrackedRetrievalTests
    {
        [Test]
        public async Task Read()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var source = new CancellationTokenSource(1000);
            var result = await Global.Services.GetRequiredService<ITrackedRetrieval>().Read(new Uri("https://www.interfax.ru/business/793954"), source.Token, encoding: Encoding.GetEncoding("windows-1251")).ConfigureAwait(false);
            Assert.IsNotEmpty(result);
        }
    }
}
