using System;
using System.Net;
using System.Threading.Tasks;

namespace Wikiled.News.Monitoring.Retriever
{
    public interface IConcurentManager
    {
        Task FinishedDownloading(Uri uri, IPAddress address);

        Task<IPAddress> StartDownloading(Uri uri);
    }
}
