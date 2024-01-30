using Abp.AutoMapper;
using Yootek.EntityDb;
using System.ComponentModel.DataAnnotations;
using Yootek.Common.Enum;

namespace Yootek.Services
{
    [AutoMap(typeof(MeterType))]
    public class CreateMeterTypeInput
    {
        public string Name { get; set; }
        [StringLength(512)] public string Description { get; set; }
        public BillType BillType { get; set; }
    }

    [AutoMap(typeof(MeterType))]
    public class CreateMeterTypeByUserInput
    {
        [StringLength(512)] public string Name { get; set; }
        [StringLength(512)] public string Description { get; set; }
        public BillType BillType { get; set; }

    }
}