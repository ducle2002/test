using System;
using Abp.AutoMapper;
using IMAX.EntityDb;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace IMAX.Services
{
    [AutoMap(typeof(MeterMonthly))]
    public class UpdateMeterMonthlyInput
    {
        public long Id { get; set; }
        public long? MeterId { get; set; }
        public DateTime? Period { get; set; }
        public int? Value { get; set; }
        [CanBeNull] public string ImageUrl { get; set; }
    }

    [AutoMap(typeof(MeterMonthly))]
    public class UpdateMeterMonthlyByUserInput
    {
        public long Id { get; set; }
        public long? MeterId { get; set; }
        public DateTime? Period { get; set; }
        public int? Value { get; set; }
        [CanBeNull] public string ImageUrl { get; set; }
    }
}