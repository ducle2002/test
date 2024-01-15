using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Organizations;
using Yootek.Organizations.Interface;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.Yootek.EntityDb.SmartCommunity.QuanLyDanCu.Citizen
{
    [Table("CitizenContract")]
    public class CitizenContract : FullAuditedEntity<long>, IMayHaveTenant, IMayHaveOrganizationUnit, IMayHaveUrban, IMayHaveBuilding
    {
        public string ApartmentCode { get; set; }
        public string ProviderFullName { get; set; }
        public string ProviderDetails { get; set; }
        public string RenterFullName { get; set; }
        public string RenterDetails { get; set; }
        public DateTime? StartRentDate { get; set; }
        public int? PaymentDay { get; set; }
        public long? RentMoney { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public int? TenantId { get; set; }
        public long? OrganizationUnitId { get; set; }
        public long? CitizenTempId { get; set; }
    }
}
