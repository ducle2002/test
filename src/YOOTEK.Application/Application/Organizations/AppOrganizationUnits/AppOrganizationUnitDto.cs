using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Organizations.AppOrganizationUnits
{
    [AutoMap(typeof(AppOrganizationUnit))]
    public class AppOrganizationUnitDto : AppOrganizationUnit
    {
        public long? UrbanId { get; set; }
        public int? CountFloor { get; set; }
    }

    [AutoMap(typeof(AppOrganizationUnit))]
    public class AppOrganizationUnitInput
    {
        public long Id { get; set; }
        public string DisplayName { get; set; }
        public string Code { get; set; }
        public long? ParentId { get; set; }
        public int? TenantId { get; set; }
        public string ProjectCode { get; set; }
        public int Type { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }


    }
}
