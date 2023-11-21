﻿using Abp.AutoMapper;
using Abp.Domain.Entities;
using IMAX.EntityDb;

namespace IMAX.Services.Dto
{
    [AutoMap(typeof(ApartmentStatus))]
    public class ApartmentStatusDto : Entity<long>
    {
        public int? TenantId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string ColorCode { get; set; }
    }
}
