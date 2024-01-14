using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using Yootek.EntityDb;
using Yootek.Common;
using static Yootek.YootekServiceBase;
namespace Yootek.Services
{
    [AutoMap(typeof(DigitalServiceCategory))]
    public class DigitalServiceCategoryDto : DigitalServiceCategory
    {
    }    
    public class GetAllDigitalServiceCategoryInputDto : CommonInputDto
    {
        		public string Keyword { get; set; }
        public FieldSortDigitalServiceCategory? OrderBy { get; set; }
    }
    public enum FieldSortDigitalServiceCategory
    {
        [FieldName("Id")]
        ID = 1,        
    }
}
