using Abp.AutoMapper;
using Abp.Domain.Entities;
using Yootek.EntityDb;
using System;
using System.ComponentModel.DataAnnotations;

namespace Yootek.Services
{
    [AutoMap(typeof(DocumentTypes))]
    public class DocumentTypesDto : Entity<long>
    {

        public int? TenantId { get; set; }
        [StringLength(512)]
        public string DisplayName { get; set; }
        [StringLength(2000)]
        public string Icon { get; set; }
        [StringLength(2000)]
        public string Description { get; set; }
        public DateTime CreationTime { get; set; }
        public long CreatorUserId { get; set; }
        public DocumentScope? Scope { get; set; }
        public bool IsGlobal { get; set; }    
    }
}