using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using IMAX.EntityDb;
using IMAX.Common;
using static IMAX.IMAXServiceBase;
using IMAX.App.ServiceHttpClient.Dto.IMAX.SmartCommunity.WorkDtos;

namespace IMAX.Services
{
    [AutoMap(typeof(DigitalServices))]
    public class DigitalServicesDto : DigitalServices
    {
        		public string UrbanText { get; set; }
		public string CategoryText { get; set; }
        public List<DigitalServiceDetailsDto> ServiceDetails { get; set; }
    }   
    public enum TypeActionUpdateStateWork
    {
        START_DOING = 1,
        COMPLETE = 2,
        CANCEL = 3,
        REOPEN = 4,
    }
    public class GetAllDigitalServicesInputDto : CommonInputDto
    {
        		public string Keyword { get; set; }
		public  long UrbanId { get; set; }
		public  long Category { get; set; }
        public FieldSortDigitalServices? OrderBy { get; set; }
    }
    public enum FieldSortDigitalServices
    {
        [FieldName("Id")]
        ID = 1,        
    }
}
