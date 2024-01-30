using Abp.AutoMapper;
using Abp.Domain.Entities;
using Yootek.EntityDb;
using Yootek.Organizations.Interface;
using System;
using System.Collections.Generic;

namespace Yootek.Services.Dto
{
    [AutoMap(typeof(Apartment))]
    public class GetApartmentDetailDto : Entity<long>, IMayHaveUrban, IMayHaveBuilding
    {
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public string ApartmentCode { get; set; }
        public long? BuildingId { get; set; }
        public string? BuildingName { get; set; }
        public long? UrbanId { get; set; }
        public string? UrbanName { get; set; }
        public decimal Area { get; set; }
        public long FloorId { get; set; }
        public string FloorName { get; set; }
        public long BlockId { get; set; }
        public string BlockName { get; set; }
        public DateTime CreationTime { get; set; }
        public long TypeId { get; set; }
        public string TypeName { get; set; }
        public long StatusId { get; set; }
        public string StatusName { get; set; }
        public string? ProvinceCode { get; set; }
        public string? DistrictCode { get; set; }
        public string? WardCode { get; set; }
        public string Address { get; set; }
        public string OwnerName { get; set; }
        public string OwnerPhoneNumber { get; set; }
        public List<MemberOfApartmentDto> Members { get; set; }
        public List<GetAllBillConfigDto>? ListBillConfig { get; set; }

    }

    public class MemberOfApartmentDto : Entity<long>
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public RELATIONSHIP? Relationship { get; set; }
        public int? Generation { get; set; }
        public bool? IsStayed { get; set; }
    }
}
