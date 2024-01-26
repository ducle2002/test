using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Yootek.EntityDb;
using Yootek.Organizations.Interface;
using JetBrains.Annotations;
using System.Collections.Generic;

namespace Yootek.Services.Dto
{
    [AutoMap(typeof(Apartment))]
    public class UpdateApartmentInput : EntityDto<long>, IMayHaveUrban, IMayHaveBuilding
    {
        [CanBeNull] public string Name { get; set; }
        [CanBeNull] public string ImageUrl { get; set; }
        [CanBeNull] public string ApartmentCode { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        [CanBeNull] public string Properties { get; set; }
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
        public List<BillConfigProperties>? BillConfigDetail { get; set; }
        public List<BillConfigProperties>? ListBillConfig { get; set; }

    }
}
