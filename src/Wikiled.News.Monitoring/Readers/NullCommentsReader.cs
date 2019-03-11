using System;
using System.Reactive.Linq;
using Wikiled.News.Monitoring.Data;
using Wikiled.News.Monitoring.Retriever;

namespace Wikiled.News.Monitoring.Readers
{
    public class NullCommentsReader : ICommentsReader
    {
        public NullCommentsReader(ITrackedRetrieval retrieval, ArticleDefinition definition)
        {
        }

        public IObservable<CommentData> ReadAllComments()
        {
            return Observable.Empty<CommentData>();
        }
    }
}
