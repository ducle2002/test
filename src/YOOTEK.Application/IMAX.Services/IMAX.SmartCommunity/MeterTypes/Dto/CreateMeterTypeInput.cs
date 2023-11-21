using Abp.AutoMapper;
using IMAX.EntityDb;
using System.ComponentModel.DataAnnotations;

namespace IMAX.Services
{
    [AutoMap(typeof(MeterType))]
    public class CreateMeterTypeInput
    {
        public string Name { get; set; }
        [StringLength(512)] public string Description { get; set; }
    }

    [AutoMap(typeof(MeterType))]
    public class CreateMeterTypeByUserInput
    {
        [StringLength(512)] public string Name { get; set; }
        [StringLength(512)] public string Description { get; set; }
    }
}