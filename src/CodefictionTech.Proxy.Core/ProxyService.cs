using System;
using System.Net.Http;

namespace CodefictionTech.Proxy.Core
{
    public class ProxyService
    {
        public ProxyService(HttpClient httpClient, SharedProxyOptions sharedProxyOptions)
        {
            Options = sharedProxyOptions ?? throw new ArgumentNullException(nameof(sharedProxyOptions));

            Client = httpClient;
        }

        public SharedProxyOptions Options { get; private set; }
        internal HttpClient Client { get; private set; }
    }
}
