using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.EntityDb
{
    [Table("HomeDevices")]
    public class HomeDevice : FullAuditedEntity<long>, IMayHaveTenant
    {
        [StringLength(256)]
        public string Name { get; set; }
        [StringLength(256)]
        public string EquipmentCompany { get; set; }
        [StringLength(2000)]
        public string DevId { get; set; }
        [StringLength(256)]
        public string DevIP { get; set; }
        [StringLength(256)]
        public string TypeDevice { get; set; }
        public string Properties { get; set; }
        [StringLength(2000)]
        public string SmartHomeCode { get; set; }
        [StringLength(2000)]
        public string ImageUrl { get; set; }
        public int? TenantId { get; set; }
        [StringLength(2000)]
        public string VisualDeviceId { get; set; }
    }
}
