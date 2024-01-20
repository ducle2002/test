using Yootek.Common;
using Yootek.Organizations.Interface;
using System;


namespace Yootek.Yootek.Services.Yootek.SmartSocial.BusinessNEW.BusinessDto.GridViewBusiness
{
    public class Location
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public Location(double lati, double lon)
        {
            Latitude = lati;
            Longitude = lon;
        }
    }


    public class GetObjectInputDto : CommonInputDto, IMayHaveUrban, IMayHaveBuilding
    {

        public long? Id { get; set; }
        public int? FormId { get; set; }
        public int? FormCase { get; set; } // Kiểu get dữ liệu
        public int? Type { get; set; }
        public int? GroupType { get; set; }
        public bool? IsDataStatic { get; set; }
        public DateTime? FromDay { get; set; }
        public DateTime? ToDay { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public ComboQuery Combo { get; set; }
        public bool? IsAdminCreate { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
    }

    /*public class ComboQuery
    {
        public bool IsActive { get; set; }
        public int[] FormIds { get; set; }
        public bool IsUnionFormId { get; set; }

    }*/

}
