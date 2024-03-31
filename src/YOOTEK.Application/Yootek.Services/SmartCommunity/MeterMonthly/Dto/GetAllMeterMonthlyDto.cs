using System;
using Abp.AutoMapper;
using Yootek.Common;
using Yootek.EntityDb;
using JetBrains.Annotations;
using static Yootek.YootekServiceBase;

namespace Yootek.Services
{
    [AutoMap(typeof(MeterMonthly))]
    public class GetAllMeterMonthlyDto : CommonInputDto
    {
        public int? MinValue { get; set; }
        public int? MaxValue { get; set; }
        //public DateTime? FromMonth { get; set; }
        //public DateTime? ToMonth { get; set; }
        public long? MeterTypeId { get; set; }
        public DateTime? Period { get; set; }
        public OrderByMeterByMonth OrderBy { get; set; }

        public int? State { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public string ApartmentCode { get; set; }
        public long? MeterId { get; set; }
        public bool? IsClosed { get; set; }
    }

    public enum OrderByMeterByMonth
    {
        [FieldName("ApartmentCode")]
        APARTMENT_CODE = 1,
        [FieldName("Period")]
        PERIOD = 2
    }
}