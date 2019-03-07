using System;
using System.Threading;
using Wikiled.News.Monitoring.Data;

namespace Wikiled.News.Monitoring.Readers
{
    public interface ICommentsReader
    {
        IObservable<CommentData> ReadAllComments(CancellationToken token);
    }
}