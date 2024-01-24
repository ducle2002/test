
using Abp.AutoMapper;
using Yootek.EntityDb;
using System.ComponentModel.DataAnnotations;


namespace Yootek.Services.Dto
{
    public class DeviceInput
    {
        public long Id { get; set; }
        [StringLength(256)]
        public string Name { get; set; }

        public string DeviceCode { get; set; }

        public int? Type { get; set; }

        public string Url { get; set; }

        public int? Port { get; set; }

        public string HomeserverAvailables { get; set; }

        public string Producer { get; set; }

        public string HomeDeviceId { get; set; }

        public string Parameters { get; set; }
        public string ImageUrl { get; set; }
    }


    [AutoMap(typeof(Device))]
    public class DeviceDto : Device
    {

    }
}
