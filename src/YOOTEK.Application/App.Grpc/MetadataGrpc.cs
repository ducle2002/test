using Abp.Runtime.Session;
using Grpc.Core;

namespace Yootek.App.Grpc
{
    public static class MetadataGrpc 
    {
        public static Metadata MetaDataHeader(IAbpSession _appSession)
        {
            var userId = _appSession.UserId > 0 ? _appSession.UserId.ToString() : "";
            var tenantId = _appSession.TenantId > 0 ? _appSession.TenantId.ToString() : "";
            var _metadata = new Metadata();

            _metadata.Add("userId", userId);
            _metadata.Add("tenantId", tenantId);

            return _metadata;
        }
    }
}
