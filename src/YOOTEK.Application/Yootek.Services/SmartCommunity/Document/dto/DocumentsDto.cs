using Abp.AutoMapper;
using Abp.Domain.Entities;
using Yootek.EntityDb;
using Yootek.Organizations.Interface;
using System;
using System.ComponentModel.DataAnnotations;

namespace Yootek.Services
{
    [AutoMap(typeof(Documents))]
    public class DocumentsDto : Entity<long>, IMayHaveUrban, IMayHaveBuilding
    {
        public int? TenantId { get; set; }
        [StringLength(1000)]
        public string DisplayName { get; set; }
        [StringLength(2000)]
        public string FileUrl { get; set; }
        [StringLength(2000)]
        public string Link { get; set; }
        public DocumentScope? Scope { get; set; }
        public bool IsGlobal { get; set; }
        public long DocumentTypeId { get; set; }
        public long? BuildingId { get; set; }
        public string? BuildingName { get; set; }
        public long? UrbanId { get; set; }
        public string? UrbanName { get; set; }

        public string DocumentTypeName { get; set; }

        public DateTime CreationTime { get; set; }
        public long CreatorUserId { get; set; }
    }
}