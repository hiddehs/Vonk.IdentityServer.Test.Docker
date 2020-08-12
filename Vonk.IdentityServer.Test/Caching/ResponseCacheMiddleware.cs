using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Vonk.IdentityServer.Test.Support;

namespace Vonk.IdentityServer
{
    internal class ResponseCacheMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ResponseCacheMiddleware> _logger;

        public ResponseCacheMiddleware(RequestDelegate next, ILogger<ResponseCacheMiddleware> logger)
        {
            Check.NotNull(next, nameof(next));
            Check.NotNull(logger, nameof(logger));
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            Check.NotNull(httpContext, nameof(httpContext));

            await _next(httpContext);

            httpContext.Response.GetTypedHeaders().CacheControl =
            new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
            {
                NoStore = true
            };
            httpContext.Response.Headers.Add("pragma", "no-cache");

            _logger.LogDebug("ResponseCacheMiddleware - Added CacheControl and Pragma header");
        }
    }
}
