using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Vonk.IdentityServer
{
    internal class ResponseCacheMiddleware
    {
        private readonly RequestDelegate _next;

        public ResponseCacheMiddleware(RequestDelegate next)
        {
            if (next == null)
                throw new ArgumentNullException(nameof(next));

            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext == null)
                throw new ArgumentNullException(nameof(httpContext));

            httpContext.Response.GetTypedHeaders().CacheControl =
            new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
            {
                NoStore = true
            };

            httpContext.Response.Headers.Add("pragma", "no-cache");

            await _next(httpContext);
        }
    }
}
