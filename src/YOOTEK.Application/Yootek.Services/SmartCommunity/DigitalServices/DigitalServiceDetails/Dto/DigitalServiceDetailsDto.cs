using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using Yootek.EntityDb;
using Yootek.Common;
using static Yootek.YootekServiceBase;
using System.ComponentModel.DataAnnotations;

namespace Yootek.Services
{
    [AutoMap(typeof(DigitalServiceDetails))]
    public class DigitalServiceDetailsDto : DigitalServiceDetails
    {
        		public string ServicesText { get; set; }
    }
    [AutoMap(typeof(DigitalServiceDetails))]
    public class DigitalServiceDetailsGridDto 
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public long? ServicesId { get; set; }
        public long Price { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public int Total { get; set; }
    }
    public class GetAllDigitalServiceDetailsInputDto : CommonInputDto
    {
        		public string Keyword { get; set; }
		public  long   ServicesId { get; set; }
        public FieldSortDigitalServiceDetails? OrderBy { get; set; }
    }
    public class GetAllGridDigitalServiceDetailsInputDto
    {
        public long ServicesId { get; set; }
        public FieldSortDigitalServiceDetails? OrderBy { get; set; }
    }
    public enum FieldSortDigitalServiceDetails
    {
        [FieldName("Id")]
        ID = 1,        
    }
}
