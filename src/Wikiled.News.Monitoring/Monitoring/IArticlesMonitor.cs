﻿using System;
using Wikiled.News.Monitoring.Data;

namespace Wikiled.News.Monitoring.Monitoring
{
    public interface IArticlesMonitor : IDisposable
    {
        IObservable<Article> NewArticles();

        IObservable<Article> MonitorUpdates();
    }
}