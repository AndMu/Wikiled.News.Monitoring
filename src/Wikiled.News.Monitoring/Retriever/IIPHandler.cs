using System.Net;
using System.Threading.Tasks;

namespace Wikiled.News.Monitoring.Retriever
{
    public interface IIPHandler
    {
        Task<IPAddress> GetAvailable();

        Task Release(IPAddress ipAddress);
    }
}
