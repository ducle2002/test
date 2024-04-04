using Abp.AutoMapper;
using Yootek.EntityDb;
using Yootek.Organizations.Interface;
using System.Collections.Generic;
using Yootek.Common.Enum;
using Yootek.Services.SmartCommunity.ExcelBill.Dto;

namespace Yootek.Services.Dto
{
    [AutoMap(typeof(Apartment))]
    public class CreateApartmentInput : IMayHaveUrban, IMayHaveBuilding
    {
        public string Name { get; set; }
        public string? ImageUrl { get; set; }
        public string ApartmentCode { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public string? Properties { get; set; }
        public decimal? Area { get; set; }
        public long? BlockId { get; set; }
        public long? FloorId { get; set; }
        public long? TypeId { get; set; }
        public long? StatusId { get; set; }
        public string? Description { get; set; }
        public string? ProvinceCode { get; set; }
        public string? DistrictCode { get; set; }
        public string? WardCode { get; set; }
        public string? Address { get; set; }
        public long[]? BillConfigId { get; set; }
        public List<BillConfigProperties> ListBillConfig { get; set; }
    }
    public class BillConfigProperties
    {
        public BillType? BillType { get; set; }
        public BillProperites Properties { get; set; }
    }
}
