using System;
using Abp.Domain.Entities;
using Yootek.Common;
using Yootek.Organizations.Interface;
using static Yootek.YootekServiceBase;

namespace Yootek.Services.Dto
{
    public class GetAllApartmentInput : CommonInputDto, IMayHaveUrban, IMayHaveBuilding
    {
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public long? StatusId { get; set; }
        public long? TypeId { get; set; }
        public long? FloorId { get; set; }
        public OrderByApartment? OrderBy { get; set; }
    }
    public enum OrderByApartment
    {
        [FieldName("Name")]
        NAME = 1,
        [FieldName("ApartmentCode")]
        APARTMENT_CODE = 2,
        [FieldName("CreationTime")]
        CREATION_TIME = 3,
    }

    public class GetAllApartmentByUserInput : CommonInputDto, IMayHaveUrban, IMayHaveBuilding
    {
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public long? StatusId { get; set; }
        public long? TypeId { get; set; }
        public OrderByApartment? OrderBy { get; set; }
    }
    public class GetAllApartmentDto : Entity<long>, IMayHaveUrban, IMayHaveBuilding
    {
        public string Name { get; set; }
        public string ApartmentCode { get; set; }
        public string ImageUrl { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public decimal? Area { get; set; }
        public long? StatusId { get; set; }
        public string StatusName { get; set; }
        public long? TypeId { get; set; }
        public string TypeName { get; set; }
        public DateTime CreationTime { get; set; }
        public string? BuildingName { get; set; }
        public string? UrbanName { get; set; }
        public string OwnerName { get; set; }
        public string OwnerPhoneNumber { get; set; }
        public int NumberOfMember { get; set; }
        public long? FloorId { get; set; }
        public string? FloorName { get; set; }
        public long? BlockId { get; set; }
        public string? BlockName { get; set; }
        public long[]? BillConfigId { get; set; }
        public string? BillConfig { get; set; }
        public string? RenterName { get; set; }


    }
}
