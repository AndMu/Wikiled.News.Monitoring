using NUnit.Framework;
using System;
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
            var source = new CancellationTokenSource(1000);
            var result = await Global.Services.GetRequiredService<ITrackedRetrieval>().Read(new Uri( "http://www.bbc.co.uk"), source.Token).ConfigureAwait(false);
            Assert.IsNotEmpty(result);
        }
    }
}
