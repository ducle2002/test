using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.Yootek.EntityDb.Smarthome.Device
{
    [Table("HomeGateways")]
    public class HomeGateway : FullAuditedEntity<long>, IMayHaveTenant
    {
        public string Properties { get; set; }
        [StringLength(256)]
        public string Type { get; set; }
        [StringLength(1000)]
        public string IpAddress { get; set; }
        [StringLength(256)]
        public string SmarthomeCode { get; set; }
        public long SmartHomeId { get; set; }
        public int? TenantId { get; set; }
    }

}
