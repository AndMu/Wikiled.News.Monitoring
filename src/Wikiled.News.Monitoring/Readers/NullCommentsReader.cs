using System;
using System.Reactive.Linq;
using Wikiled.News.Monitoring.Data;
using Wikiled.News.Monitoring.Retriever;

namespace Wikiled.News.Monitoring.Readers
{
    public class NullCommentsReader : ICommentsReader
    {
        public IObservable<CommentData> ReadAllComments(ITrackedRetrieval reader, ArticleDefinition article)
        {
            return Observable.Empty<CommentData>();
        }
    }
}
