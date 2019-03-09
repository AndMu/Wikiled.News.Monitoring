using System;
using Wikiled.News.Monitoring.Data;

namespace Wikiled.News.Monitoring.Monitoring
{
    public interface IArticlesMonitor
    {
        IObservable<Article> Start();

        IObservable<Article> Monitor();
    }
}