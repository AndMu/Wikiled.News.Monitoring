using System;
using System.Threading.Tasks;
using Wikiled.News.Monitoring.Data;

namespace Wikiled.News.Monitoring.Monitoring
{
    public interface IArticlesMonitor
    {
        Task Initialize();

        IObservable<Article> Start();

        IObservable<Article> Monitor();
    }
}