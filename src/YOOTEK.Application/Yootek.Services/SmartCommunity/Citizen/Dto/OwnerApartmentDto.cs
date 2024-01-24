using Abp.Application.Services.Dto;
using Yootek.EntityDb;
using Yootek.Organizations.Interface;

namespace Yootek.Services.Dto
{
    public class OwnerApartmentDto : EntityDto<long>, IMayHaveUrban, IMayHaveBuilding
    {
        public string FullName { get; set; }
        public long? UserId { get; set; }
        public long CitizenTempId { get; set; }
        public string ApartmentCode { get; set; }
        public decimal? ApartmentAreas { get; set; }
        public long? OrganizationUnitId { get; set; }
        public int? TenantId { get; set; }
        public RELATIONSHIP? RelationShip { get; set; }
        public bool IsStayed { get; set; }
        public int? OwnerGeneration { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
    }
}
