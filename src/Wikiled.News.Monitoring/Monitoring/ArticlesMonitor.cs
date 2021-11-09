using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wikiled.News.Monitoring.Config;
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

        private readonly ConcurrentDictionary<string, bool> scannedLookup = new ConcurrentDictionary<string, bool>();

        private readonly ConcurrentDictionary<string, IArticleDataReader> readersTable = new ConcurrentDictionary<string, IArticleDataReader>();

        private readonly Func<IArticleDataReader> readerFact;

        private readonly IDefinitionTransformer transformer;

        private readonly MonitoringConfig config;

        private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();
        
        public ArticlesMonitor(ILogger<ArticlesMonitor> logger,
                               IScheduler scheduler,
                               IFeedsHandler handler,
                               Func<IArticleDataReader> reader,
                               IDefinitionTransformer transformer, 
                               MonitoringConfig config) 
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            this.handler = handler ?? throw new ArgumentNullException(nameof(handler));
            this.readerFact = reader ?? throw new ArgumentNullException(nameof(reader));
            this.transformer = transformer ?? throw new ArgumentNullException(nameof(transformer));
            this.config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public IObservable<Article> GetCurrentArticles()
        {
            logger.LogDebug("GetCurrentArticles");
            var scanFeed = handler.GetArticles(config.DaysCutOff)
                                  .Where(item => !tokenSource.IsCancellationRequested)
                                  .Select(ArticleReceived)
                                  .Merge()
                                  .Where(item => item != null);
            return scanFeed;
        }

        public IObservable<Article> NewArticlesStream()
        {
            logger.LogDebug("NewArticlesStream");
            var scanFeed = handler.GetArticles(config.DaysCutOff).RepeatAfterDelay(TimeSpan.FromMinutes(config.ScanTime), scheduler)
                                  .Where(item => !scanned.ContainsKey(item.Id) && !tokenSource.IsCancellationRequested)
                                  .Select(ArticleReceived)
                                  .Merge()
                                  .Where(item => item != null);
            return scanFeed;
        }

        public IObservable<Article> MonitorUpdatesStream()
        {
            logger.LogDebug("MonitorUpdatesStream");
            return Observable.Interval(TimeSpan.FromHours(4), scheduler)
                .Where(item => !tokenSource.IsCancellationRequested)
                .Select(item => Updated().ToObservable(scheduler))
                .Merge()
                .Merge();
        }

        private IEnumerable<Task<Article>> Updated()
        {
            var now = DateTime.UtcNow;
            logger.LogDebug("Monitor updates: pending articles {0} with {1} comments", scanned.Count, scanned.Select(item => item.Value.Comments.Length));
            var old = scanned.Where(item => now.Subtract(item.Value.DateTime).Days >= config.KeepDays).ToArray();
            foreach (var pair in old)
            {
                scanned.TryRemove(pair.Key, out _);
            }

            return scanned.Select(item => Refresh(item.Value));
        }

        private async Task<Article> Refresh(Article article)
        {
            try
            {
                var reader = readersTable.GetOrAdd(article.Definition.Url.Host, s => readerFact());
                var comments = await reader.ReadComments(article.Definition, tokenSource.Token).ConfigureAwait(false);
                article.RefreshComments(comments);
                return article;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed");
            }

            return null;
        }

        private async Task<Article> ArticleReceived(ArticleDefinition definition)
        {
            try
            {
                var transformed = transformer.Transform(definition);
                logger.LogDebug("ArticleReceived: {0}({1})", transformed.Title, transformed.Id);
                if (scannedLookup.TryGetValue(transformed.Id, out _))
                {
                    logger.LogDebug("Article already processed: {0}", transformed.Id);
                    return null;
                }

                scannedLookup[transformed.Id] = true;
                var reader = readersTable.GetOrAdd(transformed.Url.Host, s => readerFact());
                var result = await reader.Read(transformed, tokenSource.Token).ConfigureAwait(false);
                scanned[transformed.Id] = result;
                logger.LogDebug("ArticleReceived - DONE: {0}({1})", transformed.Title, transformed.Id);
                return result;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed");
            }

            return null;
        }

        public void Dispose()
        {
            logger.LogDebug("Dispose");
            tokenSource.Cancel();
            tokenSource?.Dispose();
        }
    }
}
