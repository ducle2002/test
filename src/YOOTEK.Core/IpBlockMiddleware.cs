using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Yootek
{
    public class IpBlockMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IIpBlockingService _ipBlockingService;

        public IpBlockMiddleware(RequestDelegate next, IIpBlockingService ipBlockingService)
        {
            _next = next;
            _ipBlockingService = ipBlockingService;
        }

        public async Task Invoke(HttpContext context)
        {
            var remoteIp = context.Connection.RemoteIpAddress;

            var isBlocked = _ipBlockingService.IsBlocked(remoteIp!);

            if (isBlocked)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            await _next.Invoke(context);
        }
    }
}