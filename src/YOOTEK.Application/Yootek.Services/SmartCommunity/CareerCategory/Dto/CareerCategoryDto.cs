using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using Yootek.EntityDb;
using Yootek.Common;
using static Yootek.YootekServiceBase;
namespace Yootek.Services
{
    [AutoMap(typeof(CareerCategory))]
    public class CareerCategoryDto : CareerCategory
    {
    }    
    public class GetAllCareerCategoryInputDto : CommonInputDto
    {
        		public string Keyword { get; set; }
        public FieldSortCareerCategory? OrderBy { get; set; }
    }
    public enum FieldSortCareerCategory
    {
        [FieldName("Id")]
        ID = 1,        
    }
}
