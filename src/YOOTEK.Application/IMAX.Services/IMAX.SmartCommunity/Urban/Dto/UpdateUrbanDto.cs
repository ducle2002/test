﻿using Abp.Application.Services.Dto;
using IMAX.Organizations;
using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMAX.Services
{

    [AutoMap(typeof(AppOrganizationUnit))]
    public class UpdateUrbanDto : EntityDto<long>
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
    }
}
