using System;
using Abp.Domain.Entities;
using Abp.Organizations;
using Yootek.Common;

namespace YOOTEK.Yootek.Services
{
    public class GetAllBillEmailHistory : Entity<long>, IMayHaveOrganizationUnit
    {
        public long? CitizenTempId { get; set; }
        public string ApartmentCode { get; set; }
        public DateTime Period { get; set; }
        public long? OrganizationUnitId { get; set; }
        public string EmailTemplate { get; set; }
        public DateTime CreationTime { get; set; }
        public long? CreatorUserId { get; set; }
        public string CreatorUserName { get; set; }
        public string BuildingName { get; set; }
        public string UrbanName { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public string OwnerName { get; set; }
    }
    public class BillEmailHistoryInput : CommonInputDto
    {
        public DateTime? CreationTime { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public DateTime? Period { get; set; }
        public string ApartmentCode { get; set; }

    }
}
