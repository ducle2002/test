using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yootek.EntityDb;
using Yootek.Common;
using Abp.Application.Services.Dto;
using Abp.Domain.Entities.Auditing;

namespace Yootek.Services
{
    [AutoMap(typeof(Position))]
    public class PositionDto : Position
    {
        public long? BuildingId {get; set; }
    }
    [AutoMap(typeof(Position))]
    public class PositionInput : CommonInputDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public bool IsDeleted { get; set; }
        public string Description { get; set; }
        public int? TenantId { get; set; }
        public string DisplayName { get; set; }
        public string? Code { get; set; }
        public long ParentId { get; set; }
        public long? OrganizationUnitId { get; set; }
    }

    public class GetPositionInput : CommonInputDto
    {
        public int? TenantId { get; set; }
        public long? OrganizationUnitId { get; set; }
        public long? BuildingId { get; set; }
    }
    public class PositionListDto : EntityDto, IHasCreationTime
    {
        public string Name { get; set; }

        public string DisplayName { get; set; }

        public DateTime CreationTime { get; set; }
    }
}
