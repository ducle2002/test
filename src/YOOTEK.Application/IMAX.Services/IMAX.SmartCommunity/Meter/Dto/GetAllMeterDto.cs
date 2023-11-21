using IMAX.Common;
using JetBrains.Annotations;

namespace IMAX.IMAX.Services.IMAX.SmartCommunity.Meter.dto
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