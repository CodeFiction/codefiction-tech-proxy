using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace CodefictionTech.Proxy.Core.Contracts
{
    public interface IWebSocketRequestService
    {
        Task<bool> AcceptProxyWebSocketRequest(HttpContext context, Uri destinationUri);
    }
}