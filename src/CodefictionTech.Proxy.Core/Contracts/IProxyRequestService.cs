using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace CodefictionTech.Proxy.Core.Contracts
{
    public interface IProxyRequestService
    {
        /// <summary>
        /// Forwards current request to the specified destination uri.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="destinationUri">Destination Uri</param>
        Task ProxyRequest(HttpContext context, Uri destinationUri);
    }
}