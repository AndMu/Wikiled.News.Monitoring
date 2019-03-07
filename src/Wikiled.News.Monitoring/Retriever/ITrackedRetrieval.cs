using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Wikiled.News.Monitoring.Retriever
{
    public interface ITrackedRetrieval
    {
        Task Authenticate(Uri uri, string data, CancellationToken token, Action<HttpWebRequest> modify = null);

        Task<string> Read(Uri uri, CancellationToken token, Action<HttpWebRequest> modify = null);

        Task ReadFile(Uri uri, Stream stream, CancellationToken token);
    }
}