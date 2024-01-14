using Yootek.Common;
using JetBrains.Annotations;

namespace Yootek.Yootek.Services.Yootek.SmartCommunity.Meter.dto
{
    public class GetAllMeterDto : CommonInputDto
    {
        public long? MeterTypeId { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        [CanBeNull]
        public string ApartmentCode { get; set; }
    }
}