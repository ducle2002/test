using Abp.Domain.Entities;
using Yootek.Organizations.Interface;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.EntityDb
{
    public enum FormViewType
    {
        PreView = 1,
        CheckView = 2
    }

    public enum GroupType
    {
        Sport = 1,
        Shopping = 2,
        Community = 3
    }


    [Table("LocalService")]
    public class LocalService : Entity<long>, IMayHaveTenant, IMayHaveUrban, IMayHaveBuilding
    {
        public TypeBookingEnum? TypeBooking { get; set; }
        public int? TenantId { get; set; }
        [StringLength(256)]
        public string Name { get; set; }
        public string Icon { get; set; }
        public GroupType? GroupType { get; set; }
        public int? Type { get; set; }
        public FormViewType? FormViewType { get; set; }
        public long? OrganizationUnitId { get; set; }
        public bool? AllowPayment { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
    }
}
