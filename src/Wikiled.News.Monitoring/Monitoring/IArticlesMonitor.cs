using System;
using System.Threading;
using Wikiled.News.Monitoring.Data;

namespace Wikiled.News.Monitoring.Monitoring
{
    public interface IArticlesMonitor
    {
        IObservable<Article> Start(CancellationToken token);

        IObservable<Article> Monitor(CancellationToken token);
    }
}