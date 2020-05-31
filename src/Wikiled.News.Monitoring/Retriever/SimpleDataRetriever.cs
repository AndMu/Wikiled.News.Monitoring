using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Wikiled.News.Monitoring.Retriever
{
    public sealed class SimpleDataRetriever : IDataRetriever
    {
        private Encoding defaultEncoding = Encoding.GetEncoding("utf-8");

        private readonly HttpState httpStateRequest = new HttpState();

        private readonly ILogger<SimpleDataRetriever> logger;

        private readonly IIPHandler manager;

        private Stream readStream;

        private HttpWebResponse responseReading;

        public SimpleDataRetriever(ILogger<SimpleDataRetriever> logger, IIPHandler manager, Uri uri)
        {
            Timeout = 2 * 60 * 1000;
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.manager = manager ?? throw new ArgumentNullException(nameof(manager));
            DocumentUri = uri ?? throw new ArgumentNullException(nameof(uri));
        }

        public Action<HttpWebRequest> Modifier { get; set; }

        public CookieCollection AllCookies
        {
            get => httpStateRequest.CookieContainer.GetCookies(DocumentUri);
            set
            {
                if (value != null)
                {
                    httpStateRequest.CookieContainer.Add(value);
                }
            }
        }

        public bool AllowGlobalRedirection { get; set; }

        public ICredentials Credentials { get; set; }

        public string Data { get; private set; }

        public Uri DocumentUri { get; }

        public IPAddress Ip { get; private set; }

        public string Referer { get; set; }

        public HttpWebRequest Request => httpStateRequest.HttpRequest;

        public Uri ResponseUri { get; private set; }

        public bool Success { get; private set; }

        public int Timeout { get; set; }

        public void Dispose()
        {
            responseReading?.Close();
            responseReading = null;
        }

        public async Task PostData(Tuple<string, string>[] parameters, CancellationToken token, bool prepareCall = true)
        {
            var postData = string.Empty;
            foreach (var parameter in parameters)
            {
                if (postData.Length > 0)
                {
                    postData += "&";
                }

                postData += $"{parameter.Item1}={parameter.Item2}";
            }

            await PostData(postData, token, prepareCall).ConfigureAwait(false);
        }

        public async Task PostData(string postData, CancellationToken token, bool prepareCall = true)
        {
            try
            {
                if (prepareCall)
                {
                    await PrepareCall(HttpProtocol.POST).ConfigureAwait(false);
                }

                var encoding = new ASCIIEncoding();
                var data = encoding.GetBytes(postData);
                using (var newStream = httpStateRequest.HttpRequest.GetRequestStream())
                {
                    // Send the data.
                    newStream.Write(data, 0, data.Length);
                    responseReading = (HttpWebResponse)httpStateRequest.HttpRequest.GetResponse();
                    await StartReading().ConfigureAwait(false);
                    newStream.Close();
                }
            }
            catch (Exception)
            {
                if (Ip != null)
                {
                    await manager.Release(Ip).ConfigureAwait(false);
                }

                throw;
            }
        }

        public async Task ReceiveData(CancellationToken token, Stream stream = null)
        {
            try
            {
                readStream = stream;
                await PrepareCall().ConfigureAwait(false);
                responseReading = (HttpWebResponse)await httpStateRequest.HttpRequest.GetResponseAsync().ConfigureAwait(false);
                await StartReading().ConfigureAwait(false);
            }
            catch (Exception)
            {
                if (Ip != null)
                {
                    await manager.Release(Ip).ConfigureAwait(false);
                }

                throw;
            }
        }

        private async Task PrepareCall(HttpProtocol protocol = HttpProtocol.GET)
        {
            logger.LogDebug("Download: {0}", DocumentUri);
            CreateRequest(protocol);
            Modifier?.Invoke(httpStateRequest.HttpRequest);
            Ip = await manager.GetAvailable().ConfigureAwait(false);
            httpStateRequest.HttpRequest.ServicePoint.BindIPEndPointDelegate =
                (servicePoint, endPoint, target) =>
                {
                    logger.LogInformation("BindIp: {1} - {0}", DocumentUri, Ip);
                    return new IPEndPoint(Ip, 0);
                };
        }

        private static bool ValidateRemoteCertificate(object sender,
                                                      X509Certificate certificate,
                                                      X509Chain chain,
                                                      SslPolicyErrors policyErrors)
        {
            return true;
        }

        private void CreateRequest(HttpProtocol protocol)
        {
            logger.LogDebug("CreateRequest: {0} ({1})", DocumentUri, protocol);

            ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;

            // Open the requested URL            
            httpStateRequest.HttpRequest = (HttpWebRequest)WebRequest.Create(DocumentUri.AbsoluteUri);
            httpStateRequest.HttpRequest.Method = protocol.ToString();
            httpStateRequest.HttpRequest.AllowAutoRedirect = true;
            httpStateRequest.HttpRequest.Credentials = Credentials;
            httpStateRequest.HttpRequest.MaximumAutomaticRedirections = 10;
            httpStateRequest.HttpRequest.Referer = Referer;
            httpStateRequest.HttpRequest.Accept = "text/html, application/xhtml+xml, */*";
            httpStateRequest.HttpRequest.UserAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; Trident/5.0)";
            httpStateRequest.HttpRequest.KeepAlive = false;
            httpStateRequest.HttpRequest.Timeout = Timeout;
            httpStateRequest.HttpRequest.CookieContainer = httpStateRequest.CookieContainer;
            httpStateRequest.HttpRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
        }

        private async Task ReadData()
        {
            var webResponse = httpStateRequest.HttpResponse;
            ResponseUri = webResponse.ResponseUri;
            if (!AllowGlobalRedirection &&
                string.Compare(
                    DocumentUri.Host,
                    httpStateRequest.ResponseHost,
                    StringComparison.OrdinalIgnoreCase) !=
                0)
            {
                logger.LogWarning("URI from another host is not supported", httpStateRequest.ResponseHost);
                return;
            }

            foreach (Cookie cookie in webResponse.Cookies)
            {
                AllCookies.Add(cookie);
            }

            if (readStream != null)
            {
                await webResponse.GetResponseStream().CopyToAsync(readStream).ConfigureAwait(false);
            }

            using (var stream = new StreamReader(webResponse.GetResponseStream(), GetEncoding(webResponse)))
            {
                Data = await stream.ReadToEndAsync().ConfigureAwait(false);
            }
        }

        private Encoding GetEncoding(HttpWebResponse webResponse)
        {
            try
            {
                if (!string.IsNullOrEmpty(webResponse.ContentEncoding) &&
                    !webResponse.ContentEncoding.Contains("gzip"))
                {
                    return Encoding.GetEncoding(webResponse.ContentEncoding);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Encoding resolution");
            }

            return defaultEncoding;
        }

        private async Task ReadResponse()
        {
            if (httpStateRequest.HttpResponse == null)
            {
                return;
            }

            if (!AllowGlobalRedirection &&
                string.Compare(
                    httpStateRequest.ResponseHost,
                    httpStateRequest.RequestHost,
                    StringComparison.OrdinalIgnoreCase) !=
                0)
            {
                logger.LogWarning(
                    "{0} redirected to another host {1} and that is not supported",
                    httpStateRequest.HttpRequest.RequestUri,
                    httpStateRequest.HttpResponse.ResponseUri);
                Success = false;
                return;
            }

            await ReadData().ConfigureAwait(false);
            httpStateRequest.HttpResponse.Close();
            Success = true;
        }

        private async Task StartReading()
        {
            try
            {
                logger.LogDebug("StartReading: {0}", DocumentUri);
                // End of the Asynchronous request.
                httpStateRequest.HttpResponse = responseReading;
                httpStateRequest.HttpResponse.Cookies = AllCookies;
                await ReadResponse().ConfigureAwait(false);
                logger.LogDebug("Page processing completed: {0} on {1}", httpStateRequest.HttpRequest.RequestUri, Ip);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Page processing failed: {0} on {1}", httpStateRequest.HttpRequest.RequestUri, Ip);
                throw;
            }
            finally
            {
                await manager.Release(Ip).ConfigureAwait(false);
                httpStateRequest.HttpResponse?.Close();
                ServicePointManager.ServerCertificateValidationCallback -= ValidateRemoteCertificate;
            }
        }
    }
}