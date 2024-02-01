using System;
using Abp.AutoMapper;
using Yootek.EntityDb;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace Yootek.Services
{
    [AutoMap(typeof(MeterMonthly))]
    public class UpdateMeterMonthlyInput
    {
        public long Id { get; set; }
        public long? MeterId { get; set; }
        public DateTime? Period { get; set; }
        public int? Value { get; set; }
        public int? FirstValue { get; set; }
        public bool? IsClosed { get; set; }
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