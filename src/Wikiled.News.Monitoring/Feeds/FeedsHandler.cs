using CodeHollow.FeedReader;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
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
            if (feeds.Length == 0)
            {
                throw new ArgumentException("Value cannot be an empty collection.", nameof(feeds));
            }
            
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IObservable<ArticleDefinition> GetArticles()
        {
            return Observable.Create<ArticleDefinition>(
                async observer =>
                {
                    logger.LogInformation("Getting articles...");
                    var tasks = new List<(FeedName Feed, Task<Feed> Task)>();
                    foreach (var feed in feeds)
                    {
                        var task = FeedReader.ReadAsync(feed.Url);
                        tasks.Add((feed, task));
                    }

                    foreach (var task in tasks)
                    {
                        var result = await task.Task;
                        foreach (var item in result.Items)
                        {
                            var article = new ArticleDefinition();
                            article.Url = new Uri(item.Link);
                            article.Id = item.Id;
                            article.Date = item.PublishingDate;
                            article.Title = item.Title;
                            article.Feed = task.Feed;
                            article.Element = item.SpecificItem.Element;
                            logger.LogDebug("Found definition {0}...", article.Title);
                            observer.OnNext(article);
                        }
                    }

                    observer.OnCompleted();
                });
        }
    }
}
