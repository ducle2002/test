using Abp.AutoMapper;
using Yootek.EntityDb;
using System;
using System.Collections.Generic;

namespace Yootek.Services.Dto
{

    public class RoomSmartHomeInput
    {
    }

    public class GetAllRoomInput
    {
        public long? SmartHomeId { get; set; }
    }

}