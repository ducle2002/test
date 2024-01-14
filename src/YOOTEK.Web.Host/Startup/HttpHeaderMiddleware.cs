using Abp.Runtime.Session;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Yootek.Web.Host.Startup
{
    public class HttpHeaderMiddleware
    {
        private readonly RequestDelegate _next;

        private IAbpSession _appSession;

        public HttpHeaderMiddleware(
            RequestDelegate next,
            IAbpSession appSession
            )
        {
            _next = next;
            _appSession = appSession;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                var userId = _appSession.UserId > 0 ? _appSession.UserId.ToString() : "";
                var tenantId = _appSession.TenantId > 0 ? _appSession.TenantId.ToString() : "";
                httpContext.Request.Headers.Add("userId", userId);
                httpContext.Request.Headers.Add("tenantId", tenantId);
            }
            catch (Exception e)
            {
            }
            await _next(httpContext);
        }
    }
}
