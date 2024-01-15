using Yootek.Common;

namespace Yootek.Services.SmartSocial.Advertisements
{
    public class GetAllAmenitiesComboInput : CommonInputDto
    {
        public long ProviderId { get; set; }
        public int BusinessType { get; set; }
        public bool IsPagination { get; set; }

    }
}
