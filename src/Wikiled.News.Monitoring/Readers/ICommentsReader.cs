using System;
using Wikiled.News.Monitoring.Data;
using Wikiled.News.Monitoring.Retriever;

namespace Wikiled.News.Monitoring.Readers
{
    public interface ICommentsReader
    {
        IObservable<CommentData> ReadAllComments(ITrackedRetrieval reader, ArticleDefinition article);
    }
}