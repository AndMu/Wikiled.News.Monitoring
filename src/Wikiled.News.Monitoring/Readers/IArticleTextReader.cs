using System.Threading;
using System.Threading.Tasks;
using Wikiled.News.Monitoring.Data;
using Wikiled.News.Monitoring.Retriever;

namespace Wikiled.News.Monitoring.Readers
{
    public interface IArticleTextReader
    {
        Task<ArticleContent> ReadArticle(ITrackedRetrieval reader, ArticleDefinition definition, CancellationToken token);
    }
}