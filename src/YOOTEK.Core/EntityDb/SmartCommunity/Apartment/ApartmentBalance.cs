using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System.ComponentModel.DataAnnotations;
using Yootek.Common.Enum;
using Yootek.Organizations.Interface;

namespace YOOTEK.EntityDb
{
    public class ApartmentBalance : FullAuditedEntity<long>, IMayHaveTenant, IMayHaveUrban, IMayHaveBuilding
    {
        public int? TenantId { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        [StringLength(1000)] public string CustomerName { get; set; }
        [StringLength(1000)] public string ApartmentCode { get; set; }
        public BillType? BillType { get; set; }
        public decimal Amount { get; set; }
        public long? CitizenTempId { get; set; }
        public long? UserBillId { get; set; }
        public EBalanceAction EBalanceAction { get; set; }
    }

}
