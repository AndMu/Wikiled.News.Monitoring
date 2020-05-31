using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Wikiled.News.Monitoring.Retriever
{
    public class IPHandler : IIPHandler
    {
        private readonly ILogger<IPHandler> logger;

        private readonly SemaphoreSlim semaphore;

        private readonly ConcurrentQueue<IPAddress> addressed = new ConcurrentQueue<IPAddress>();

        public IPHandler(ILogger<IPHandler> logger, RetrieveConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (config.MaxConcurrent < 1)
            {
                throw new ArgumentException("Max concurrency can not be less than 1", nameof(config.MaxConcurrent));
            }

            this.logger = logger;

            var ips = config.Ips?.Length >= 1 ? config.Ips.Select(IPAddress.Parse).ToArray() : new[] { IPAddress.Any };

            for (var i = 0; i < config.MaxConcurrent; i++)
            {
                foreach (var ip in ips)
                {
                    logger.LogInformation("Adding local IP: {0}", ip);
                    addressed.Enqueue(ip);
                }
            }

            semaphore = new SemaphoreSlim(addressed.Count);
        }

        public async Task<IPAddress> GetAvailable()
        {
            for (; ; )
            {
                await semaphore.WaitAsync().ConfigureAwait(false);
                if (addressed.TryDequeue(out var item))
                {
                    return item;
                }
            }
        }

        public Task Release(IPAddress ipAddress)
        {
            addressed.Enqueue(ipAddress);
            semaphore.Release();
            return Task.CompletedTask;
        }
    }
}
