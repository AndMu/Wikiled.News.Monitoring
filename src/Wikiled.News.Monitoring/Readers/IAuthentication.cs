using System.Threading;
using System.Threading.Tasks;

namespace Wikiled.News.Monitoring.Readers
{
    public interface IAuthentication
    {
        Task<bool> Authenticate(CancellationToken token);
    }
}
