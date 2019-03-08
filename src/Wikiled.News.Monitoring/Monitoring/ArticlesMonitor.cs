﻿using System;
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

        private readonly ConcurrentDictionary<string, bool> scannedLookup = new ConcurrentDictionary<string, bool>();

        private readonly IArticleDataReader reader;

        private readonly IDefinitionTransformer transformer;

        private const int keepDays = 5;

        private readonly IAuthentication authentication;

        private bool isInitialized;

        public ArticlesMonitor(ILogger<ArticlesMonitor> logger,
                               IScheduler scheduler,
                               IFeedsHandler handler,
                               IArticleDataReader reader,
                               IDefinitionTransformer transformer,
                               IAuthentication authentication)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            this.handler = handler ?? throw new ArgumentNullException(nameof(handler));
            this.reader = reader ?? throw new ArgumentNullException(nameof(reader));
            this.transformer = transformer ?? throw new ArgumentNullException(nameof(transformer));
            this.authentication = authentication ?? throw new ArgumentNullException(nameof(authentication));
        }

        public async Task Initialize(CancellationToken token)
        {
            logger.LogDebug("Initialize");
            var result = await authentication.Authenticate(token).ConfigureAwait(false);
            if (!result)
            {
                logger.LogError("Authentication failed");
                throw new Exception("Authentication failed");
            }

            isInitialized = true;
        }

        public IObservable<Article> Start()
        {
            logger.LogDebug("Start");
            if (!isInitialized)
            {
                throw new Exception("Monitoring is not initialized");
            }

            var scanFeed = handler.GetArticles().RepeatAfterDelay(TimeSpan.FromHours(1), scheduler)
                                  .Where(item => !scanned.ContainsKey(item.Id))
                                  .Select(ArticleReceived)
                                  .Merge()
                                  .Where(item => item != null);

            return scanFeed;
        }

        public IObservable<Article> Monitor()
        {
            logger.LogDebug("Monitor");
            return Observable.Interval(TimeSpan.FromHours(4), scheduler)
                             .Select(item => Updated().ToObservable(scheduler))
                             .Merge()
                             .Merge();
        }

        private IEnumerable<Task<Article>> Updated()
        {
            var now = DateTime.UtcNow;
            var old = scanned.Where(item => now.Subtract(item.Value.DateTime).Days >= keepDays).ToArray();
            foreach (var pair in old)
            {
                scanned.TryRemove(pair.Key, out _);
            }

            return scanned.Select(item => Refresh(item.Value));
        }

        private async Task<Article> Refresh(Article article)
        {
            var comments = await reader.ReadComments(article.Definition).ToArray();
            article.RefreshComments(comments);
            return article;
        }

        private async Task<Article> ArticleReceived(ArticleDefinition article)
        {
            var transformed = transformer.Transform(article);
            logger.LogDebug("ArticleReceived: {0}({1})", transformed.Title, transformed.Id);
            if (scannedLookup.TryGetValue(transformed.Id, out _))
            {
                logger.LogDebug("Article already processed: {0}", transformed.Id);
                return null;
            }

            scannedLookup[transformed.Id] = true;
            try
            {
                var result = await reader.Read(transformed, CancellationToken.None).ConfigureAwait(false);
                scanned[transformed.Id] = result;
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
