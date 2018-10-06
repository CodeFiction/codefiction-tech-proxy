using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;

namespace CodefictionTech.Proxy.Core
{
    public static class ProxyServiceCollectionExtensions
    {
        public static IServiceCollection AddProxy(this IServiceCollection services, Func<SharedProxyOptions> configureOptions = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            SharedProxyOptions sharedProxyOptions = configureOptions?.Invoke() ?? new SharedProxyOptions()
            {
                MessageHandler = new HttpClientHandler {AllowAutoRedirect = false, UseCookies = false}
            };

            services.AddSingleton(sharedProxyOptions);
            services.AddHttpClient<ProxyService>().ConfigurePrimaryHttpMessageHandler(() => sharedProxyOptions.MessageHandler);

            return services;
        }
    }
}
