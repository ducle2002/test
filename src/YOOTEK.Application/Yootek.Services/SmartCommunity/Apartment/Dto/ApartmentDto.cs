using Abp.AutoMapper;
using Abp.Domain.Entities;
using Yootek.EntityDb;
using Yootek.Organizations.Interface;
using System;
using System.Collections.Generic;

namespace Yootek.Services.Dto
{
    [AutoMap(typeof(Apartment))]
    public class ApartmentDto : Entity<long>, IMayHaveUrban, IMayHaveBuilding
    {
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public string ApartmentCode { get; set; }
        public long? BuildingId { get; set; }
        public string? BuildingName { get; set; }
        public long? UrbanId { get; set; }
        public string? UrbanName { get; set; }
        public decimal? Area { get; set; }
        public DateTime CreationTime { get; set; }
        public long CreatorUserId { get; set; }
        public Citizen CitizenRepresentative { get; set; }
        public long? TypeId { get; set; }
        public long? StatusId { get; set; }
        public string? ProvinceCode { get; set; }
        public string? DistrictCode { get; set; }
        public string? WardCode { get; set; }
        public List<CitizenRole> Citizens { get; set; }
        public int? CurrentCitizenCount { get; set; }
    }

    [AutoMap(typeof(Apartment))]
    public class ApartmentCreateBillDto : Entity<long>, IMayHaveUrban, IMayHaveBuilding
    {
        public string ApartmentCode { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public decimal? Area { get; set; }
        public long? TypeId { get; set; }
        public string CustomerName { get; set; }
        public string BillConfig { get; set; }
    }
}
