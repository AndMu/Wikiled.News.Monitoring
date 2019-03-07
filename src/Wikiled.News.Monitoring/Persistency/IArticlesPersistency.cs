using System.Threading.Tasks;
using Wikiled.News.Monitoring.Data;

namespace Wikiled.News.Monitoring.Persistency
{
    public interface IArticlesPersistency
    {
        Task<bool> Save(Article article);
    }
}