﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;

namespace CodefictionTech.Proxy.Core
{
    /// <summary>
    /// Proxy Middleware
    /// </summary>
    public class ProxyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ProxyOptions _options;

        public ProxyMiddleware(RequestDelegate next, IOptions<ProxyOptions> options)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            if (options.Value.Scheme == null)
            {
                throw new ArgumentException("Options parameter must specify scheme.", nameof(options));
            }
            if (!options.Value.Host.HasValue)
            {
                throw new ArgumentException("Options parameter must specify host.", nameof(options));
            }

            _next = next;
            _options = options.Value;
        }

        public Task Invoke(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var uri = new Uri(UriHelper.BuildAbsolute(_options.Scheme, _options.Host, _options.PathBase, context.Request.Path, context.Request.QueryString.Add(_options.AppendQuery)));
            return context.ProxyRequest(uri);
        }
    }
}
