using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CodeHollow.FeedReader;
using Microsoft.Extensions.Logging;
using Wikiled.News.Monitoring.Data;

namespace Wikiled.News.Monitoring.Feeds
{
    public class FeedsHandler : IFeedsHandler
    {
        private readonly ILogger<FeedsHandler> logger;

        private readonly FeedName[] feeds;

        public FeedsHandler(ILogger<FeedsHandler> logger, FeedName[] feeds)
        {
            this.feeds = feeds ?? throw new ArgumentNullException(nameof(feeds));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IObservable<ArticleDefinition> GetArticles(int cuttoff = 10)
        {
            return Observable.Create<ArticleDefinition>(
                async observer =>
                {
                    try
                    {
                        await foreach (var article in ProcessFeed(cuttoff))
                        {
                            observer.OnNext(article);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Feed failure");
                    }

                    observer.OnCompleted();
                });
        }

        private async IAsyncEnumerable<ArticleDefinition> ProcessFeed(int cuttoff)
        {
            logger.LogTrace("Getting articles...");
            var tasks = new List<(FeedName Feed, Task<Feed> Task)>();
            foreach (var feed in feeds)
            {
                var task = GetFeed(new Uri(feed.Url));
                tasks.Add((feed, task));
            }

            var cutOff = DateTime.Today.AddDays(-cuttoff);
            foreach (var task in tasks)
            {
                var result = await task.Task.ConfigureAwait(false);
                foreach (var item in result.Items)
                {
                    var article = new ArticleDefinition();
                    article.Url = new Uri(item.Link);
                    article.Id = item.Id ?? item.Title;
                    article.Date = item.PublishingDate;
                    article.Title = item.Title;
                    article.Feed = task.Feed;
                    article.Topic = task.Feed?.Category;
                    article.Element = item.SpecificItem.Element;
                    if (article.Date < cutOff)
                    {
                        logger.LogTrace("Ignoring old definition {0} [{1}]...", article.Title, article.Date);
                        continue;
                    }

                    logger.LogTrace("Found definition {0} [{1}] and id [{2}]...", article.Title, article.Date, article.Id);
                    yield return article;
                }
            }
        }

        private async Task<Feed> GetFeed(Uri uri)
        {
            logger.LogTrace("Quering {0}...", uri);
            using var client = new HttpClient();
            client.BaseAddress = uri;
            var feedData = await client.GetStringAsync(string.Empty);
            return FeedReader.ReadFromString(feedData);
        }
    }
}
