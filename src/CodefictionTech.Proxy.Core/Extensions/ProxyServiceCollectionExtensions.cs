using System;
using System.Net.Http;
using CodefictionTech.Proxy.Core.Contracts;
using CodefictionTech.Proxy.Core.Options;
using CodefictionTech.Proxy.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CodefictionTech.Proxy.Core.Extensions
{
    public static class ProxyServiceCollectionExtensions
    {
        public static IServiceCollection AddProxy(this IServiceCollection services, Func<HttpMessageHandler> customClientHandlerSetupAction = null, Action<WebSocketOptions> webSocketOptionsSetupAction = null)
        {
            return AddProxy<HttpRequestService, WebSocketRequestService>(services, customClientHandlerSetupAction, webSocketOptionsSetupAction);
        }

        public static IServiceCollection AddProxyWithCustomHttpService<THttpRequestService>(this IServiceCollection services, Func<HttpMessageHandler> customClientHandlerSetupAction = null, Action<WebSocketOptions> webSocketOptionsSetupAction = null)
            where THttpRequestService : class, IHttpRequestService
        {
            return AddProxy<THttpRequestService, WebSocketRequestService>(services, customClientHandlerSetupAction, webSocketOptionsSetupAction);
        }

        public static IServiceCollection AddProxyWithCustomWebSocketService<TWebSocketService>(this IServiceCollection services, Func<HttpMessageHandler> customClientHandlerSetupAction = null, Action<WebSocketOptions> webSocketOptionsSetupAction = null)
            where TWebSocketService : class, IWebSocketRequestService
        {
            return AddProxy<HttpRequestService, TWebSocketService>(services, customClientHandlerSetupAction, webSocketOptionsSetupAction);
        }

        public static IServiceCollection AddProxy<THttpRequestService, TWebSocketService>(this IServiceCollection services, Func<HttpMessageHandler> customClientHandlerSetupAction = null, Action<WebSocketOptions> webSocketOptionsSetupAction = null) 
            where  THttpRequestService : class, IHttpRequestService 
            where TWebSocketService : class, IWebSocketRequestService
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var webSocketOptions = new WebSocketOptions();

            webSocketOptionsSetupAction?.Invoke(webSocketOptions);

            services.AddSingleton(webSocketOptions);
            services.AddTransient<IProxyRequestService, ProxyRequestService>();
            services.AddTransient<IWebSocketRequestService, TWebSocketService>();

            services.AddHttpClient<IHttpRequestService, THttpRequestService>().ConfigurePrimaryHttpMessageHandler(() =>
            {
                HttpMessageHandler httpMessageHandler = null;

                if (customClientHandlerSetupAction == null)
                {
                    httpMessageHandler = new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false };
                }
                else
                {
                    httpMessageHandler = customClientHandlerSetupAction();
                }

                return httpMessageHandler;
            });


            return services;
        }
    }
}
