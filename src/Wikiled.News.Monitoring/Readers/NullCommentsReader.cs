using System;
using System.Reactive.Linq;
using Wikiled.News.Monitoring.Data;

namespace Wikiled.News.Monitoring.Readers
{
    public class NullCommentsReader : ICommentsReader
    {
        public IObservable<CommentData> ReadAllComments()
        {
            return Observable.Empty<CommentData>();
        }
    }
}
