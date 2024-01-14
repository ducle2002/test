using Yootek.Common;

namespace Yootek.Services.Dto
{
    public class GetAllOwnerApartmentInput : CommonInputDto
    {
        public long? OrganizationUnitId { get; set; }
        public bool? IsStayed { get; set; }
        public string ApartmentCode { get; set; }
        public bool IsAllMember { get; set; }
    }
}
