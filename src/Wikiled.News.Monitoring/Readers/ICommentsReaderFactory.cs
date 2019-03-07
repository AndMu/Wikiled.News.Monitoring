using Wikiled.News.Monitoring.Data;

namespace Wikiled.News.Monitoring.Readers
{
    public interface ICommentsReaderFactory
    {
        ICommentsReader Create(ArticleDefinition article);
    }
}