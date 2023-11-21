using System;
using IMAX.Common;
using JetBrains.Annotations;

namespace IMAX.IMAX.Services.IMAX.SmartCommunity.MeterMonthly.dto
{
    public class GetAllMeterMonthlyDto : CommonInputDto
    {
        public int? MinValue;
        public int? MaxValue;
        public DateTime? FromMonth;
        public DateTime? ToMonth;
        public long? MeterTypeId { get; set; }
    }
}