using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Wikiled.News.Monitoring.Data;
using Wikiled.News.Monitoring.Extensions;
using Wikiled.News.Monitoring.Feeds;
using Wikiled.News.Monitoring.Readers;

namespace Wikiled.News.Monitoring.Monitoring
{
    public class ArticlesMonitor : IArticlesMonitor
    {
        private readonly IFeedsHandler handler;

        private readonly IScheduler scheduler;

        private readonly ILogger<ArticlesMonitor> logger;

        private readonly ConcurrentDictionary<string, Article> scanned = new ConcurrentDictionary<string, Article>();

        private readonly IArticleDataReader reader;

        public ArticlesMonitor(ILogger<ArticlesMonitor> logger, IScheduler scheduler, IFeedsHandler handler, IArticleDataReader reader)
        {
            this.logger = logger;
            this.scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            this.handler = handler ?? throw new ArgumentNullException(nameof(handler));
            this.reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        public IObservable<Article> Start(CancellationToken token)
        {
            logger.LogDebug("Start");
            var scanFeed = handler.GetArticles().RepeatAfterDelay(TimeSpan.FromHours(1), scheduler)
                                  .Where(item => !scanned.ContainsKey(item.Id))
                                  .Select(item => ArticleReceived(item, token))
                                  .Merge()
                                  .Where(item => item != null);

            return scanFeed;
        }

        public IObservable<Article> Monitor(CancellationToken token)
        {
            return Observable.Interval(TimeSpan.FromHours(4), scheduler)
                             .Select(item => Updated(token).ToObservable(scheduler))
                             .Merge()
                             .Merge();
        }

        private IEnumerable<Task<Article>> Updated(CancellationToken token)
        {
            var now = DateTime.UtcNow;
            var old = scanned.Where(item => now.Subtract(item.Value.DateTime).Days >= 2).ToArray();
            foreach (var pair in old)
            {
                scanned.TryRemove(pair.Key, out _);
            }

            return scanned.Where(item => now.Subtract(item.Value.DateTime).Hours >= 2).Select(item => reader.Read(item.Value.Definition, token));
        }

        private async Task<Article> ArticleReceived(ArticleDefinition article, CancellationToken token)
        {
            logger.LogDebug("ArticleReceived: {0}({1})", article.Topic, article.Id);
            if (scanned.TryGetValue(article.ToString(), out var _))
            {
                logger.LogDebug("Article already processed: {0}", article.Id);
                return null;
            }

            try
            {
                var result = await reader.Read(article, token).ConfigureAwait(false);
                scanned[article.ToString()] = result;
                return result;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed");
            }

            return null;
        }
    }
}
