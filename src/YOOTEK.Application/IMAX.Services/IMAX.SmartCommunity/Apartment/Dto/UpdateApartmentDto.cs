using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using IMAX.EntityDb;
using IMAX.Organizations.Interface;
using JetBrains.Annotations;

namespace IMAX.Services.Dto
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
    }
}
