using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Yootek.Organizations.Interface;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.EntityDb
{
    [Table("ParkingIntegratedCustomers")]
    public class IntegratedParkingCustomer : FullAuditedEntity<long>, IMayHaveTenant, IMayHaveUrban, IMayHaveBuilding
    {
        [StringLength(256)]
        public string CustomerName { get; set; }
        [StringLength(256)]
        public string CustomerID { get; set; }
        [StringLength(256)]
        public string CustomerCode { get; set; }
        public int? TenantId { get; set; }
        [StringLength(256)]
        public string CustomerGroupID { get; set; }
        [StringLength(1000)]
        public string Address { get; set; }
        [StringLength(256)]
        public string IDNumber { get; set; }
        [StringLength(256)]
        public string Mobile { get; set; }
        public long? CitizenTempId { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
    }
}
