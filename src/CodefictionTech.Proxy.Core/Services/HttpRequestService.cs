using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CodefictionTech.Proxy.Core.Contracts;
using Microsoft.AspNetCore.Http;

namespace CodefictionTech.Proxy.Core.Services
{
    public class HttpRequestService : IHttpRequestService
    {
        protected const int StreamCopyBufferSize = 81920;

        private readonly HttpClient _httpClient;

        public HttpRequestService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task SendProxyHttpRequest(HttpContext context, Uri destinationUri)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (destinationUri == null)
            {
                throw new ArgumentNullException(nameof(destinationUri));
            }

            using (var requestMessage = CreateProxyHttpRequest(context, destinationUri))
            {
                await BeforeSendRequestToOriginal(context.Request, requestMessage);

                using (var httpResponseMessage = await _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted))
                {
                    await BeforeCopyHeadersToResponse(httpResponseMessage);

                    context.Response.StatusCode = (int)httpResponseMessage.StatusCode;
                    foreach (var header in httpResponseMessage.Headers)
                    {
                        context.Response.Headers[header.Key] = header.Value.ToArray();
                    }

                    foreach (var header in httpResponseMessage.Content.Headers)
                    {
                        context.Response.Headers[header.Key] = header.Value.ToArray();
                    }

                    // SendAsync removes chunking from the response. This removes the header so it doesn't expect a chunked response.
                    context.Response.Headers.Remove("transfer-encoding");

                    await CopyContentToResponse(context, httpResponseMessage);
                }
            }
        }

        protected virtual async Task BeforeSendRequestToOriginal(HttpRequest httpRequest, HttpRequestMessage httpRequestMessage)
        {
            return;
        }

        protected virtual async Task BeforeCopyHeadersToResponse(HttpResponseMessage httpResponseMessage)
        {
            return;
        }

        protected virtual async Task CopyContentToResponse(HttpContext context, HttpResponseMessage httpResponseMessage)
        {
            using (Stream responseStream = await httpResponseMessage.Content.ReadAsStreamAsync())
            {
                await responseStream.CopyToAsync(context.Response.Body, StreamCopyBufferSize, context.RequestAborted);
            }
        }

        private static HttpRequestMessage CreateProxyHttpRequest(HttpContext context, Uri uri)
        {
            var request = context.Request;

            var requestMessage = new HttpRequestMessage();
            var requestMethod = request.Method;
            if (!HttpMethods.IsGet(requestMethod) &&
                !HttpMethods.IsHead(requestMethod) &&
                !HttpMethods.IsDelete(requestMethod) &&
                !HttpMethods.IsTrace(requestMethod))
            {
                var streamContent = new StreamContent(request.Body);
                requestMessage.Content = streamContent;
            }

            // Copy the request headers
            foreach (var header in request.Headers)
            {
                if (!requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()))
                {
                    requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                }
            }

            requestMessage.Headers.Host = uri.Authority;
            requestMessage.RequestUri = uri;
            requestMessage.Method = new HttpMethod(request.Method);

            return requestMessage;
        }
    }
}
