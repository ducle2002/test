using Abp.AutoMapper;
using Yootek.EntityDb;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace Yootek.Services.Dto
{

    public class GetSettingInputDto
    {
        public long? Id { get; set; }
        public string Code { get; set; }
    }
    public class SmartHomeInput
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

    [AutoMap(typeof(SmartHome))]
    public class SmartHomeDto : SmartHome
    {

        //public long? SmarthomeServerId { get; set; }
        //public string HomeServerName { get; set; }
        //public List<string> ListRoom { get; set; }
    }

    [AutoMap(typeof(Apartment))]
    public class ApartmentImportExcelDto : Apartment
    {

    }

    public class CreateSmartHomeInput
    {
        public string Properties { get; set; }
        public long UserId { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string HouseDetail { get; set; }
        public string ApartmentCode { get; set; }
    }

    public class CreateHomeGatewayInput
    {
        public string Properties { get; set; }
        public string IpAddress { get; set; }
        public string SmarthomeCode { get; set; }
    }

    public class MemberSmarthomeInput
    {
        public long UserId { get; set; }
        public string SmarthomeCode { get; set; }

    }

    public class AdminCreateSmartHomeInput
    {
        public string Properties { get; set; }
        public long? UserId { get; set; }
    }

    public class UpdateSmartHomeInput
    {
        public string SmartHomeCode { get; set; }
        public string Properties { get; set; }
        public string HouseDetail { get; set; }
        public string Name { get; set; }
        public string ApartmentCode { get; set; }

    }

    public class CreateMemberInput
    {
        public string SmarthomeCode { get; set; }
        public string UserNameOrEmail { get; set; }
    }


    public class GetRoomInput
    {
        public int? FormCase { get; set; }
        public long? SmartHomeId { get; set; }
        public long? FloorSmartHomeId { get; set; }

    }

    public class DataSearch
    {
        public string data { get; set; }

    }


    public class ExecuteActionInput
    {
        public string DevId { get; set; }
        public string DeviceType { get; set; }
        public string ActionName { get; set; }
    }
    //public class GetSmartHomeInput
    //{
    //    public long OrganizationUnitId {get; set; }

    //}

    public class ImportExcelSmartHome
    {
        public IFormFile File { get; set; }
    }
}
