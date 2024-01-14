using System;

namespace Yootek.Services.Dto
{
    public class CitizenVerificationdto
    {
        public string ApartmentCode { get; set; }
        public long UrbanId { get; set; }
        public long BuildingId { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string FullName { get; set; }


    }
}
