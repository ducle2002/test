using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Services.Dto
{
    public class SmarthomeMobileDto
    {
        public string home_code_tuya { get; set; }
        public string home_code_server { get; set; }
        public string home_name { get; set; }
        public string img { get; set; }
        public List<RoomInfor> room_infor { get; set; }
        public List<VisualDeviceConfig> visual_device_config { get; set; }
        public List<PhysicalDeviceConfig> physical_device_config { get; set; }
    }

    public class RoomInfor
    {
        public string _id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string background { get; set; }
        public int index { get; set; }

    }


    public class VisualDeviceConfig
    {
        public string _id { get; set; }
        public string dev_name { get; set; }
        public string dev_group { get; set; }
        public string dev_type { get; set; }
        public RoomInfor room { get; set; }
    }
    public class PhysicalDeviceConfig
    {
        public string _id { get; set; }
        public string key { get; set; }
        public string type_device { get; set; }
        public PhysicalDeviceProperties property { get; set; }
    }

    public class PhysicalDeviceProperties
    {
        public string dev_id { get; set; }
        public string name { get; set; }
        public string ipGateway { get; set; }
        public Schema schema { get; set; }
    }

    public class Schema
    {
        public string strategyValue { get; set; }
        public string strategyCode { get; set; }
        public string standardType { get; set; }
        public object valueRange { get; set; }
        public string dpCode { get; set; }
        public object relationDpIdMaps { get; set; }
    }
}
