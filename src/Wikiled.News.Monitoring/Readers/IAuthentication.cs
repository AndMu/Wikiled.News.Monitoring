using System.Threading;
using System.Threading.Tasks;
using Wikiled.News.Monitoring.Retriever;

namespace Wikiled.News.Monitoring.Readers
{
    public interface IAuthentication
    {
        Task<bool> Authenticate(ITrackedRetrieval reader, CancellationToken token);
    }
}
