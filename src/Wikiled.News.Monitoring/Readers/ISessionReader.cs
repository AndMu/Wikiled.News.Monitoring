using System.Threading;
using System.Threading.Tasks;
using Wikiled.News.Monitoring.Data;

namespace Wikiled.News.Monitoring.Readers
{
    public interface ISessionReader
    {
        Task Init(CancellationToken token);

        Task<CommentData[]> ReadComments(ArticleDefinition article, CancellationToken token);

        Task<ArticleText> ReadArticle(ArticleDefinition article, CancellationToken token);
    }
}
