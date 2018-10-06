using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace CodefictionTech.Proxy.Core
{
    /// <summary>
    /// Shared Proxy Options
    /// </summary>
    public class SharedProxyOptions
    {
        /// <summary>
        /// Message handler used for http message forwarding.
        /// </summary>
        public HttpMessageHandler MessageHandler { get; set; }

        /// <summary>
        /// Allows to modify HttpRequestMessage before it is sent to the Message Handler.
        /// </summary>
        public Func<HttpRequest, HttpRequestMessage, Task> PrepareRequest { get; set; }

        /// <summary>
        /// Allows to modify HttpResponseMessage before it is sent to the CopyResponseMessageHeadersToHttpResponse.
        /// </summary>
        public Func<HttpResponseMessage, Task> BeforeCopyProxyHttpResponse { get; set; }

        /// <summary>
        /// Allows to modify response after it is sent to the CopyResponseMessageHeadersToHttpResponse.
        /// </summary>
        public Func<HttpContext, HttpResponseMessage, Task> CopyProxyHttpResponseOverride { get; set; }
    }
}