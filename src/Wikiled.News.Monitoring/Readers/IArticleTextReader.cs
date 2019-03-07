using System.Threading;
using System.Threading.Tasks;
using Wikiled.News.Monitoring.Data;

namespace Wikiled.News.Monitoring.Readers
{
    public interface IArticleTextReader
    {
        Task<ArticleText> ReadArticle(ArticleDefinition definition, CancellationToken token);
    }
}