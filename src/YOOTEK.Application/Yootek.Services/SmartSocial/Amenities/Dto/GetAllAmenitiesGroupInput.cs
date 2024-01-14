

using Yootek.Common;

namespace Yootek.Services.SmartSocial.Advertisements 
{ 
    public class GetAllAmenitiesGroupInput : CommonInputDto
    {
        public int BusinessType { get; set; }
        public bool IsPagination { get; set; }
        public long? ProviderId { get; set; }
    }
}
