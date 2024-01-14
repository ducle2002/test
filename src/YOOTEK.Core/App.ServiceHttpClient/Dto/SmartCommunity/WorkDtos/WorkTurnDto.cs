#nullable enable
using Abp.Domain.Entities;
using Yootek.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Yootek.App.ServiceHttpClient.Dto.Yootek.SmartCommunity.WorkDtos
{
    public class WorkTurnDto : Entity<long>
    {
        public int? TenantId { get; set; }
        public long WorkId { get; set; }
        public int TurnNumber { get; set; }
        [MaxLength(1000)]
        public string? Description { get; set; }
        public long CreatorUserId { get; set; }
        public DateTime CreationTime { get; set; }
    }

    public class GetAllWorkTurnsDto : CommonInputDto
    {
        public long? WorkId { get; set; }
    }

    public class GetWorkTurnByIdDto : Entity<long>
    {
    }

    public class CreateWorkTurnDto
    {
        public long WorkId { get; set; }
        [MaxLength(1000)]
        public string? Description { get; set; }
    }

    public class UpdateWorkTurnDto : Entity<long>
    {
    }

    public class DeleteWorkTurnDto : Entity<long>
    {
    }

    public class DeleteManyWorkTurnDto
    {
        public List<long>? Ids { get; set; }
    }
}
