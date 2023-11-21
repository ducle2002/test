using System;
using Abp.AutoMapper;
using IMAX.EntityDb;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace IMAX.Services
{
    [AutoMap(typeof(MeterMonthly))]
    public class CreateMeterMonthlyInput
    {
        public long? MeterId { get; set; }
        public DateTime Period { get; set; }
        public int Value { get; set; }
        [CanBeNull] public string ImageUrl { get; set; }
    }

    [AutoMap(typeof(MeterMonthly))]
    public class CreateMeterMonthlyByUserInput
    {
        public long? MeterId { get; set; }
        public DateTime Period { get; set; }
        public int Value { get; set; }
        [CanBeNull] public string ImageUrl { get; set; }

    }
}