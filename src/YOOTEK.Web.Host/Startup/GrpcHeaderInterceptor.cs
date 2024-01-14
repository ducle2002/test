using Abp.Runtime.Session;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Yootek.Web.Host.Startup
{
    public class GrpcHeaderInterceptor : Interceptor
    {
        private readonly IAbpSession _appSession;

        public GrpcHeaderInterceptor(
           )
        {
            
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
         TRequest request,
         ClientInterceptorContext<TRequest, TResponse> context,
         AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            var userId = _appSession.UserId > 0 ? _appSession.UserId.ToString() : "";
            var tenantId = _appSession.TenantId > 0 ? _appSession.TenantId.ToString() : "";
            var headers = new Metadata();
            headers.Add("userId", userId);
            headers.Add("tenantId", tenantId);
            context = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, context.Options.WithHeaders(headers));

            return continuation(request, context);
        }
    }
}
