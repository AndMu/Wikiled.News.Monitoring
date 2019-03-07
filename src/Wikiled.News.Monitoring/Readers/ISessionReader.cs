using System.Threading;
using System.Threading.Tasks;
using Wikiled.News.Monitoring.Data;

namespace Wikiled.News.Monitoring.Readers
{
    public interface ISessionReader
    {
        ICommentsReader GetCommentsReader(ArticleDefinition article);

        Task<ArticleText> ReadArticle(ArticleDefinition article, CancellationToken token);
    }
}
