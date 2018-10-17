using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using CodefictionTech.Proxy.Core.Services;
using Microsoft.AspNetCore.Http;

namespace CodefictionTech.Proxy.Services
{
    public class CodefictionHttpRequestService : HttpRequestService
    {
        private static readonly string LogRocketScript =
            @"<script src=""https://cdn.logrocket.io/LogRocket.min.js"" crossorigin=""anonymous""></script>" +
            @"<script>window.LogRocket && window.LogRocket.init('clyrcf/codefiction-tech');</script>";

        public CodefictionHttpRequestService(HttpClient httpClient) 
            : base(httpClient)
        {
        }

        protected override async Task CopyContentToResponse(HttpContext httpContext, HttpResponseMessage originalHttpResponseMessage)
        {
            HttpRequest originalRequest = httpContext.Request;
            HttpResponse httpResponse = httpContext.Response;

            HostString originalRequestHost = originalRequest.Host;
            var originalRequestHostPort = originalRequestHost.Port;

            MediaTypeHeaderValue contentType = originalHttpResponseMessage.Content.Headers.ContentType;
            var replaceContent = originalHttpResponseMessage.IsSuccessStatusCode 
                    && contentType?.MediaType != null 
                    &&contentType.MediaType.Contains("text/html");

            if (replaceContent)
            {
                var isLocalHost = originalRequestHostPort.HasValue
                                  && originalRequestHostPort.Value != 80
                                  && originalRequestHostPort.Value != 443;

                var originalHost = $"{originalRequest.Host.Host}{(isLocalHost ? $":{originalRequestHostPort.Value}" : string.Empty)}";

                var htmlContent = await originalHttpResponseMessage.Content.ReadAsStringAsync();

                htmlContent = htmlContent.Replace("codefiction.simplecast.fm", originalHost);
                htmlContent = htmlContent.Insert(htmlContent.IndexOf("<head>", StringComparison.Ordinal) + "<head>".Length, LogRocketScript);


                var byteArray = Encoding.UTF8.GetBytes(htmlContent);

                using (var memoryStream = new MemoryStream(byteArray))
                {
                    await memoryStream.CopyToAsync(httpResponse.Body, StreamCopyBufferSize, httpContext.RequestAborted);
                }
            }
            else
            {
                await base.CopyContentToResponse(httpContext, originalHttpResponseMessage);
            }
        }
    }
}
