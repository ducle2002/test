namespace Yootek.Web.Host.Controllers.ModelView
{
    public class AuditLoginInput
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string IpAddress { get; set; }
        public string MacAddress { get;set; }
        public string DeviceId { get; set; }
        public int? TenantId { get; set; }
        public string TenantName { get; set; }
        public string AppName { get; set; }
    }
}
