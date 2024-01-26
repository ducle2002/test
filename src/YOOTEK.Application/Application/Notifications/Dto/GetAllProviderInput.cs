
using Abp.Domain.Entities;

namespace Yootek.Application.Notifications.Dto
{
    public class GetAllProviderInput
    {
        public int? Type { get; set; }
        public int? GroupType { get; set; }
    }

    public class GetAllCitizenInput
    {
        public int? TenantId { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
    }

    public class GetBuildingInput
    {
        public int? TenantId { get; set; }
        public long? UrbanId { get; set; }
    }
    public class GetApartmentInput
    {
        public int? TenantId { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
    }

    public class ApartmentGetBySocialAdminDto: Entity<long>
    {
        public string ApartmentCode {  get; set; }
        public long? TenantId { get; set; }
        public long? UrbanId { get;set; }
        public long? BuildingId { get; set; }
    }
}
