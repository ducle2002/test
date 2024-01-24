using Abp.AutoMapper;
using Yootek.Common;
using Yootek.Yootek.EntityDb.SmartCommunity.QuanLyDanCu.Citizen;
using Yootek.Organizations.Interface;
using System;

namespace Yootek.Services.Dto
{
    public class CitizenContractQueryDto : CommonInputDto, IMayHaveUrban, IMayHaveBuilding
    {
        public long? OrganizationUnitId { get; set; }
        public string ApartmentCode { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
    }

    public class CitizenContractOutputDto : CitizenContract
    {
        public DateTime? DateOfBirth { get; set; }
        public string UrbanName { get; set; }
        public string Gender { get; set; }
    }

    [AutoMap(typeof(CitizenContract))]
    public class CitizenContractInputDto : CitizenContract
    {
        public DateTime? DateOfBirth { get; set; }
        public string RenterPhoneNumber { get; set; }
        public string RenterIdentityNumber { get; set; }
        public string Gender { get; set; }
    }
}
