using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Yootek.EntityDb;
using Yootek.Organizations.Interface;
using System;

namespace Yootek.Services
{
    [AutoMap(typeof(Material))]
    public class MaterialDto : EntityDto<long>, IMayHaveUrban, IMayHaveBuilding
    {
        public string MaterialName { get; set; }
        public string MaterialCode { get; set; }
        public string ImageUrl { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public long? TypeId { get; set; }
        public long? GroupId { get; set; }
        public long? UnitId { get; set; }
        public long? ProducerId { get; set; }
        public long? LocationId { get; set; }
        public int? TenantId { get; set; }
        public string SerialNumber { get; set; }
        public string Specifications { get; set; }
        public int? Amount { get; set; }
        public int? TotalPrice { get; set; }
        public string FileUrl { get; set; }
        public int? Price { get; set; }
        public DateTime? ManufacturerDate { get; set; }
        public int? WarrantyMonth { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public DateTime? UsedDate { get; set; }
        public decimal? WearRate { get; set; }
        public long? OrganizationUnitId { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public DateTime CreationTime { get; set; }
        public Guid? Key { get; set; }
        public long? MaterialId { get; set; }
        public string GroupName { get; set; }
        public string TypeName { get; set; }
        public string LocationName { get; set; }
        public string UnitName { get; set; }
        public string ProducerName { get; set; }


        // Thêm các trường cho việc thống kê theo tháng và LocationId
        public int? Month { get; set; }
        public int? Year { get; set; }
        public int? MonthlyMaterialCount { get; set; }

    }
}
