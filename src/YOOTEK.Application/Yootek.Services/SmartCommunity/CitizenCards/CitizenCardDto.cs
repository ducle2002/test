using Abp.AutoMapper;
using Yootek.Common;
using Yootek.EntityDb;
using System;


namespace Yootek.Services
{
    [AutoMap(typeof(CitizenCard))]
    public class CitizenCardDto : CitizenCard
    {
    }

    public class CitizenCardOutput : CitizenCard
    {
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string FullName { get; set; }
        public string Nationality { get; set; }
        public string IdentityNumber { get; set; }
        public string ImageUrl { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string OtherPhones { get; set; }
        public int? BirthYear { get; set; }
        public string BuildingName { get; set; }
        public string BuildingCode { get; set; }
        public string ApartmentCode { get; set; }
        public int? Type { get; set; }
    }

    public class CitizenCardInput : CommonInputDto
    {
        public long OrganizationUnitId { get; set; }
        public bool IsLocked { get; set; }
        //public int Type { get; set; }

    }
}
