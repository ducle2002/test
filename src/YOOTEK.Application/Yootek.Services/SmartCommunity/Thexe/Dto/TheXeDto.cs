using Abp.AutoMapper;
using Yootek.Common;
using Yootek.EntityDb;
using System;

namespace Yootek.Services
{
    [AutoMap(typeof(TheXe))]
    public class TheXeDto : TheXe
    {
    }

    [AutoMap(typeof(TheXe))]
    public class TheXeInputDto
    {
        public long Id { get; set; }
        public int? TenantId { get; set; }
        public string BuildingCode { get; set; }
        public long? OrganizationUnitId { get; set; }
        public string Code { get; set; }
        public string ApartmentOwnerName { get; set; }
        public string RelationShipWithApartmentOwner { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string ApartmentCode { get; set; }
        public string LicensePlate { get; set; }
        public string Description { get; set; }
        public TheXeStatus Status { get; set; }
    }

    public class GetAllTheXeInput : CommonInputDto
    {
        public long Id { get; set; }
        public long TenantId { get; set; }
        public DateTime? FromDay { get; set; }
        public DateTime? ToDay { get; set; }
        public TheXeStatus? Status { get; set; }
    }

}
