using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CodefictionTech.Proxy.Core.Contracts;
using Microsoft.AspNetCore.Http;

namespace CodefictionTech.Proxy.Core.Services
{
    public class HttpRequestService : IHttpRequestService
    {
        private const int StreamCopyBufferSize = 81920;

        private readonly HttpClient _httpClient;
        private readonly SharedProxyOptions _sharedProxyOptions;

        public HttpRequestService(HttpClient httpClient, SharedProxyOptions sharedProxyOptions)
        {
            _httpClient = httpClient;
            _sharedProxyOptions = sharedProxyOptions;
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
                var prepareRequestHandler = _sharedProxyOptions.PrepareRequest;

                if (prepareRequestHandler != null)
                {
                    await prepareRequestHandler(context.Request, requestMessage);
                }

                using (var httpResponseMessage = await _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted))
                {
                    var beforeCopyProxyHttpResponseHandler = _sharedProxyOptions.BeforeCopyProxyHttpResponse;
                    var copyProxyHttpResponseOverrideHandler = _sharedProxyOptions.CopyProxyHttpResponseOverride;

                    if (beforeCopyProxyHttpResponseHandler != null)
                    {
                        await beforeCopyProxyHttpResponseHandler(httpResponseMessage);
                    }

                    CopyResponseMessageHeadersToHttpResponse(context, httpResponseMessage);

                    if (copyProxyHttpResponseOverrideHandler == null)
                    {
                        using (var responseStream = await httpResponseMessage.Content.ReadAsStreamAsync())
                        {
                            await responseStream.CopyToAsync(context.Response.Body, StreamCopyBufferSize, context.RequestAborted);
                        }
                    }
                    else
                    {
                        await copyProxyHttpResponseOverrideHandler(context, httpResponseMessage);
                    }
                }
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

        private static void CopyResponseMessageHeadersToHttpResponse(HttpContext context, HttpResponseMessage responseMessage)
        {
            if (responseMessage == null)
            {
                throw new ArgumentNullException(nameof(responseMessage));
            }

            var response = context.Response;

            response.StatusCode = (int)responseMessage.StatusCode;
            foreach (var header in responseMessage.Headers)
            {
                response.Headers[header.Key] = header.Value.ToArray();
            }

            foreach (var header in responseMessage.Content.Headers)
            {
                response.Headers[header.Key] = header.Value.ToArray();
            }

            // SendAsync removes chunking from the response. This removes the header so it doesn't expect a chunked response.
            response.Headers.Remove("transfer-encoding");
        }
    }
}
