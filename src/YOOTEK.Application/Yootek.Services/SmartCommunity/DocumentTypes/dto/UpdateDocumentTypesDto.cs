using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Yootek.EntityDb;
using System.ComponentModel.DataAnnotations;

namespace Yootek.Services
{
    [AutoMap(typeof(DocumentTypes))]
    public class UpdateDocumentTypesInput : EntityDto<long>
    {
        [StringLength(512)]
        public string DisplayName { get; set; }
        [StringLength(2000)]
        public string Icon { get; set; }
        [StringLength(2000)]
        public string Description { get; set; }
        public DocumentScope? Scope { get; set; }
        public bool IsGlobal { get; set; }    
    }

    [AutoMap(typeof(DocumentTypes))]
    public class UpdateDocumentTypesByUserInput : EntityDto<long>
    {
        [StringLength(512)]
        public string DisplayName { get; set; }
        [StringLength(2000)]
        public string Icon { get; set; }
        [StringLength(2000)]
        public string Description { get; set; }
        public DocumentScope? Scope { get; set; }
        public bool IsGlobal { get; set; } 
    }
}