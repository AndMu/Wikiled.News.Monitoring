using System.Threading.Tasks;

namespace Wikiled.News.Monitoring.Readers
{
    public class NullAuthentication : IAuthentication
    {
        public Task<bool> Authenticate()
        {
            return Task.FromResult(true);
        }
    }
}
