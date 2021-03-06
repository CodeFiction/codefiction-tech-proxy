﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace CodefictionTech.Proxy.Core.Contracts
{
    public interface IHttpRequestService
    {
        Task SendProxyHttpRequest(HttpContext context, Uri destinationUri);
    }
}