using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CodefictionTech.Proxy.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CodefictionTech.Proxy
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static IConfiguration Configuration { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddProxy(() =>
            {
                var options = new SharedProxyOptions();

                options.MessageHandler = new HttpClientHandler
                {
                    AllowAutoRedirect = false,
                    UseCookies = false,
                    AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
                };

                options.PrepareRequest = (originalRequest, httpRequestMessage) =>
                {
                    // httpRequestMessage.Headers.Add("X-Forwarded-Host", originalRequest.Host.Host);
                    return Task.FromResult(0);
                };

                options.BeforeCopyProxyHttpResponse = originalHttpResponseMessage =>
                {
                    return Task.FromResult(0);
                };

                options.CopyProxyHttpResponseOverride = async (httpContext, originalHttpResponseMessage) =>
                {
                    var originalRequest = httpContext.Request;
                    var httpResponse = httpContext.Response;

                    var originalRequestHost = originalRequest.Host;
                    var originalRequestHostPort = originalRequestHost.Port;

                    var localHost = originalRequestHostPort.HasValue
                                    && originalRequestHostPort.Value != 80
                                    && originalRequestHostPort.Value != 443;

                    var originalHost = $"{originalRequest.Host.Host}{(localHost ? $":{originalRequestHostPort.Value}" : string.Empty)}";
                    const int streamCopyBufferSize = 81920;
                    var contentType = originalHttpResponseMessage.Content.Headers.ContentType;

                    if (originalHttpResponseMessage.IsSuccessStatusCode && contentType?.MediaType != null && contentType.MediaType.Contains("text/html"))
                    {
                        var htmlContent = await originalHttpResponseMessage.Content.ReadAsStringAsync();

                        htmlContent = htmlContent.Replace("codefiction.simplecast.fm", originalHost);
                        byte[] byteArray = Encoding.UTF8.GetBytes(htmlContent);

                        using (MemoryStream memoryStream = new MemoryStream(byteArray))
                        {
                            await memoryStream.CopyToAsync(httpResponse.Body, streamCopyBufferSize, httpContext.RequestAborted);
                        }
                    }
                    else
                    {
                        using (var responseStream = await originalHttpResponseMessage.Content.ReadAsStreamAsync())
                        {
                            await responseStream.CopyToAsync(httpContext.Response.Body, streamCopyBufferSize, httpContext.RequestAborted);
                        }
                    }
                };

                return options;
            });

            services.AddMvc();
            services.AddResponseCompression();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}
            //else
            //{
            //    app.UseHsts();
            //}

            app.UseCors(x =>
            {
                x.AllowAnyHeader();
                x.AllowAnyMethod();
                x.AllowAnyOrigin();
            });

            app.UseDeveloperExceptionPage()
                .UseResponseCompression()
                .UseHttpsRedirection()
                .UseMvc()
                .UseWebSockets()
                .RunProxy(new Uri("https://codefiction.simplecast.fm/"));
        }
    }
}
