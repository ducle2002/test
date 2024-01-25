using Yootek.Organizations.Interface;
using System.Collections.Generic;

namespace Yootek.Services.ExportData
{
    public class ExportApartmentInput : IMayHaveUrban, IMayHaveBuilding
    {
        public List<long>? Ids { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public long? TypeId { get; set; }
        public long? StatusId { get; set; }
    }

    public class ApartmentExportDto : IMayHaveUrban, IMayHaveBuilding
    {
        public long Id { get; set; }
        public int? TenantId { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public string? CustomerName { get; set; }
        public string ApartmentCode { get; set; }
        public long? BuildingId { get; set; }
        public string? BuildingCode { get; set; }
        public long? UrbanId { get; set; }
        public string? UrbanCode { get; set; }
        public decimal? Area { get; set; }
        public long? StatusId { get; set; }
        public string StatusName { get; set; }
        public long? TypeId { get; set; }
        public string TypeName { get; set; }
        public long? FloorId { get; set; }
        public string? FloorName { get; set; }
        public long? BlockId { get; set; }
        public string? BlockName { get; set; }
        public string? ProvinceCode { get; set; }
        public string? DistrictCode { get; set; }
        public string? WardCode { get; set; }
        public string? Address { get; set; }
    }
}
