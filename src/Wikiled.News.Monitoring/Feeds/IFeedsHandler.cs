using System;
using Wikiled.News.Monitoring.Data;

namespace Wikiled.News.Monitoring.Feeds
{
    public interface IFeedsHandler
    {
        IObservable<ArticleDefinition> GetArticles(int cuttoff = 10);
    }
}