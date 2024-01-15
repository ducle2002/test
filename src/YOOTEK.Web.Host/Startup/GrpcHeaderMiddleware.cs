using Abp.Runtime.Session;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System;
using System.Threading.Tasks;

namespace Yootek.Web.Host.Startup
{
    public class GrpcHeaderMiddleware
    {
        private readonly RequestDelegate _next;

        private IAbpSession _appSession;
        private Metadata _metadata;

        public GrpcHeaderMiddleware(
            RequestDelegate next,
            IAbpSession appSession,
            Metadata metadata
            )
        {
            _next = next;
            _appSession = appSession;
            _metadata = metadata;
        }

      
        private readonly CallInvoker _callInvoker;

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {

                var userId = _appSession.UserId > 0 ? _appSession.UserId.ToString() : "";
                var tenantId = _appSession.TenantId > 0 ? _appSession.TenantId.ToString() : "";
                if (_metadata != null)
                {
                    var checkUser = _metadata.Get("userId");
                    if (checkUser != null)
                    {
                        _metadata.Remove(checkUser);
                    }

                    var checkTenant = _metadata.Get("tenantId");
                    if (checkTenant != null)
                    {
                        _metadata.Remove(checkTenant);
                    }

                    _metadata.Add("userId", userId);
                    _metadata.Add("tenantId", tenantId);
                }

              
            }
            catch(Exception e) 
            { 
            }
            await _next(httpContext);
        }
    }
}
