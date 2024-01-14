using Abp.AutoMapper;
using Yootek.EntityDb;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace Yootek.Services
{
    [AutoMap(typeof(Meter))]
    public class UpdateMeterInput
    {
        public long Id { get; set; }
        [StringLength(1000)] [CanBeNull] public string Name { get; set; }
        [StringLength(2000)] [CanBeNull] public string QrCode { get; set; }
        [StringLength(2000)] [CanBeNull] public string Code { get; set; }
        public long? MeterTypeId { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public string? ApartmentCode { get; set; }

    }

    [AutoMap(typeof(Meter))]
    public class UpdateMeterByUserInput
    {
        public long Id { get; set; } 
        public string? ApartmentCode { get; set; }
        [StringLength(1000)] [CanBeNull] public string Name { get; set; }
        [StringLength(2000)] [CanBeNull] public string QrCode { get; set; }
        [StringLength(2000)] [CanBeNull] public string Code { get; set; }
        public long? MeterTypeId { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
    }
}