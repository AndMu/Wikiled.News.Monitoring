using System.Threading;
using System.Threading.Tasks;
using Wikiled.News.Monitoring.Retriever;

namespace Wikiled.News.Monitoring.Readers
{
    public class NullAuthentication : IAuthentication
    {
        public Task<bool> Authenticate(ITrackedRetrieval reader, CancellationToken token)
        {
            return Task.FromResult(true);
        }
    }
}
