using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Wikiled.News.Monitoring.Retriever;
using Wikiled.News.Monitoring.Tests.Helpers;

namespace Wikiled.News.Monitoring.Tests.Acceptance
{
    [TestFixture]
    public class TrackedRetrievalTests
    {
        private NetworkHelper instance;

        [SetUp]
        public void Setup()
        {
            instance = new NetworkHelper();
        }

        [Test]
        public async Task Read()
        {
            var source = new CancellationTokenSource(1000);
            var result = await instance.Retrieval.Read(new Uri( "http://www.bbc.co.uk"), source.Token);
            Assert.IsNotEmpty(result);
        }
    }
}
