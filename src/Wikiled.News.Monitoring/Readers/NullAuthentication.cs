using System;
using System.Threading;
using System.Threading.Tasks;
using Wikiled.News.Monitoring.Retriever;

namespace Wikiled.News.Monitoring.Readers
{
    public class NullAuthentication : IAuthentication
    {
        public NullAuthentication(ITrackedRetrieval trackedRetrieval)
        {
        }

        public Task<bool> Authenticate(CancellationToken token)
        {
            return Task.FromResult(true);
        }
    }
}
