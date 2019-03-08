using System.Threading;
using System.Threading.Tasks;

namespace Wikiled.News.Monitoring.Readers
{
    public class NullAuthentication : IAuthentication
    {
        public Task<bool> Authenticate(CancellationToken token)
        {
            return Task.FromResult(true);
        }
    }
}
