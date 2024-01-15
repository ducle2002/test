using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.EntityDb
{

    public interface IDevice : IEntity<long>
    {
        public long? SmartHomeId { get; set; }
    }

    public interface IDeviceDto : IDevice
    {
    }



    [Table("DeviceSettings")]
    public class Device : FullAuditedEntity<long>, IMayHaveTenant
    {
        [StringLength(256)]
        public string Name { get; set; }

        [StringLength(256)]
        public string DeviceCode { get; set; }
        [StringLength(256)]
        public string TypeDevice { get; set; }

        [StringLength(256)]
        public string GroupDevice { get; set; }
        [StringLength(2000)]
        public string Url { get; set; }

        public int? Port { get; set; }

        [StringLength(2000)]
        public string HomeserverAvailables { get; set; }

        [StringLength(2000)]
        public string Producer { get; set; }

        [StringLength(2000)]
        public string HomeDeviceId { get; set; }
        [StringLength(2000)]
        public string ImageUrl { get; set; }

        public string Parameters { get; set; }
        public int? TenantId { get; set; }
    }
}
