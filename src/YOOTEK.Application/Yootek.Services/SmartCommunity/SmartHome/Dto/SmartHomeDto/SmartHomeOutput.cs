using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Services.Dto
{
    public class SmartHomeOutput
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string SmartHomeCode { get; set; }
        public long? UserId { get; set; }
        public string Scope { get; set; }
        public string RefreshToken { get; set; }
        public int? NumberRooms { get; set; }
        public int? NumberFloors { get; set; }
        public int? NumberDevices { get; set; }
        public int? NumberHomeServers { get; set; }
        public long? CreatorUserId { get; set; }
        public DateTime? CreationTime { get; set; }
    }

    public class SmartHomeSettingOutput
    {
        public string SmartHomecode { get; set; }
        public string Properties { get; set; }
    }

    public class RoomOutputDto
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public string Number { get; set; }

        public long? SmartHomeId { get; set; }

        public long? FloorSmartHomeId { get; set; }

        public string ImageUrl { get; set; }

        public int? NumberDevices { get; set; }
        public long? CreatorUserId { get; set; }
        public DateTime? CreationTime { get; set; }
    }


    public class UserSmarthome
    {
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string ImageUrl { get; set; }
        public bool IsAdmin { get; set; }
    }
}
