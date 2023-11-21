using Abp.AutoMapper;
using IMAX.EntityDb;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace IMAX.Services
{
    [AutoMap(typeof(MeterType))]
    public class UpdateMeterTypeInput
    {
        public long Id { get; set; }
        [CanBeNull] public string Name { get; set; }
        [StringLength(512)] [CanBeNull] public string Description { get; set; }
    }

    [AutoMap(typeof(MeterType))]
    public class UpdateMeterTypeByUserInput
    {
        public long Id { get; set; }
        [StringLength(512)] [CanBeNull] public string Name { get; set; }
        [StringLength(512)] [CanBeNull] public string Description { get; set; }
    }
}