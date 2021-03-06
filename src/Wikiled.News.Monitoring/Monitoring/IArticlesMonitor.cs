﻿using System;
using Wikiled.News.Monitoring.Data;

namespace Wikiled.News.Monitoring.Monitoring
{
    public interface IArticlesMonitor : IDisposable
    {
        IObservable<Article> GetCurrentArticles();

        IObservable<Article> NewArticlesStream();

        IObservable<Article> MonitorUpdatesStream();
    }
}