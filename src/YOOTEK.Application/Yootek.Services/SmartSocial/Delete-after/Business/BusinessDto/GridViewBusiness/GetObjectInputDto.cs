using Abp.Application.Services.Dto;
using Yootek.Common;
using System;


namespace Yootek.Services.Dto
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


    public class GetObjectInputDto : CommonInputDto
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
    }

    public class ComboQuery
    {
        public bool IsActive { get; set; }
        public int[] FormIds { get; set; }
        public bool IsUnionFormId { get; set; }

    }

}
