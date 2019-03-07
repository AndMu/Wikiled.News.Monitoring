using System.Net;

namespace Wikiled.News.Monitoring.Retriever
{
    public class HttpState
    {
        private HttpWebRequest httpRequest;

        private HttpWebResponse httpResponse;

        public CookieContainer CookieContainer { get; private set; } = new CookieContainer();

        public HttpWebRequest HttpRequest
        {
            get => httpRequest;
            set
            {
                httpRequest = value;
                RequestHost = httpRequest.RequestUri.Host;
            }
        }

        public HttpWebResponse HttpResponse
        {
            get => httpResponse;
            set
            {
                httpResponse = value;
                ResponseHost = httpResponse.ResponseUri.Host;
            }
        }

        public string RequestHost { get; private set; }

        public string ResponseHost { get; private set; }

        public void ResetCookies()
        {
            CookieContainer = new CookieContainer();
        }
    }
}
