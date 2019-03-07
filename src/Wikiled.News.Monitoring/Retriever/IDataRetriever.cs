using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Wikiled.News.Monitoring.Retriever
{
    public interface IDataRetriever : IDisposable
    {
        Action<HttpWebRequest> Modifier { get; set; }

        CookieCollection AllCookies { get; set; }

        bool AllowGlobalRedirection { get; set; }

        ICredentials Credentials { get; set; }

        string Data { get; }

        Uri DocumentUri { get; }

        IPAddress Ip { get; }

        string Referer { get; set; }

        HttpWebRequest Request { get; }

        Uri ResponseUri { get; }

        bool Success { get; }

        int Timeout { get; set; }

        Task PostData(string postData, CancellationToken token, bool prepareCall = true);

        Task PostData(Tuple<string, string>[] parameters, CancellationToken token, bool prepareCall = true);

        Task ReceiveData(CancellationToken token, Stream stream = null);
    }
}