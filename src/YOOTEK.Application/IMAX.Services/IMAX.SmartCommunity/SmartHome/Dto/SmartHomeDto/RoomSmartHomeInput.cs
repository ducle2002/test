using Abp.AutoMapper;
using IMAX.EntityDb;
using System;
using System.Collections.Generic;

namespace IMAX.Services.Dto
{

    public class RoomSmartHomeInput
    {
    }

    public class GetAllRoomInput
    {
        public long? SmartHomeId { get; set; }
    }

}