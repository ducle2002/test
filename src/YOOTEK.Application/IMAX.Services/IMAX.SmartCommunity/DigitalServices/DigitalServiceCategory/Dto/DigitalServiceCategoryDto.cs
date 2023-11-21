using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using IMAX.EntityDb;
using IMAX.Common;
using static IMAX.IMAXServiceBase;
namespace IMAX.Services
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
