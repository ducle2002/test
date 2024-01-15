using Abp.Application.Services.Dto;
using Yootek.Organizations;
using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Services
{

    [AutoMap(typeof(AppOrganizationUnit))]
    public class CreateUrbanDto : EntityDto<long>
    {
        public string ProjectCode { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string ProvinceCode { get; set; }
        public string DistrictCode { get; set; }
        public string WardCode { get; set; }
        public string DisplayName { get; set; }
        public long? ParentId { get; set; }
    }
}
