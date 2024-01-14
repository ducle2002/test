using Abp.AutoMapper;
using Yootek.EntityDb;
using System.ComponentModel.DataAnnotations;
using Yootek.Common.Enum;
using JetBrains.Annotations;

namespace Yootek.Services
{
    [AutoMap(typeof(MeterType))]
    public class UpdateMeterTypeInput
    {
        public long Id { get; set; }
        [CanBeNull] public string Name { get; set; }
        [StringLength(512)] [CanBeNull] public string Description { get; set; }
        public BillType BillType { get; set; }
    }

    [AutoMap(typeof(MeterType))]
    public class UpdateMeterTypeByUserInput
    {
        public long Id { get; set; }
        [StringLength(512)] [CanBeNull] public string Name { get; set; }
        [StringLength(512)] [CanBeNull] public string Description { get; set; }
        public BillType BillType { get; set; }
    }
}