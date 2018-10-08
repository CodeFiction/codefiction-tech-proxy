using System;
using System.Threading.Tasks;
using CodefictionTech.Proxy.Core.Contracts;
using Microsoft.AspNetCore.Http;

namespace CodefictionTech.Proxy.Core.Services
{
    public class ProxyRequestService : IProxyRequestService
    {
        private readonly IWebSocketRequestService _webSocketRequestService;
        private readonly IHttpRequestService _httpRequestService;

        public ProxyRequestService(IWebSocketRequestService webSocketRequestService, IHttpRequestService httpRequestService)
        {
            _webSocketRequestService = webSocketRequestService;
            _httpRequestService = httpRequestService;
        }

        /// <summary>
        /// Forwards current request to the specified destination uri.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="destinationUri">Destination Uri</param>
        public async Task ProxyRequest(HttpContext context, Uri destinationUri)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (destinationUri == null)
            {
                throw new ArgumentNullException(nameof(destinationUri));
            }

            if (context.WebSockets.IsWebSocketRequest)
            {
                await _webSocketRequestService.AcceptProxyWebSocketRequest(context, destinationUri);
            }
            else
            {
                await _httpRequestService.SendProxyHttpRequest(context, destinationUri);
            }
        }
    }
}
