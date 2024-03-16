using System.ComponentModel.DataAnnotations;
using Abp.AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Yootek.EntityDb;

namespace Yootek.Services
{
    [AutoMap(typeof(Meter))]
    public class CreateMeterInput
    {
        [StringLength(1000)] public string Name { get; set; }
        [StringLength(2000)] public string Code { get; set; }
        public long? MeterTypeId { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public string? ApartmentCode { get; set; }
    }

    [AutoMap(typeof(Meter))]
    public class CreateMeterByUserInput
    {
        [StringLength(1000)] public string Name { get; set; }
        [StringLength(2000)] public string Code { get; set; }
        public long? MeterTypeId { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public string? ApartmentCode { get; set; }
    }
    public class ImportCreateMeterInput
    {
        public IFormFile File { get; set; }
        [FromQuery] public long? MeterTypeId { get; set; }

    }
}