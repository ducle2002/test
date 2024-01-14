using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Abp.Organizations;

namespace Yootek.Organizations.Dto
{
    public class CreateOrganizationUnitInput
    {
        public long? ParentId { get; set; }

        [Required]
        [StringLength(OrganizationUnit.MaxDisplayNameLength)]
        public string DisplayName { get; set; }
        public APP_ORGANIZATION_TYPE Type { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public bool IsManager { get; set; }
        public string ProjectCode { get; set; }
        public APP_ORGANIZATION_TYPE[] Types { get; set; }
    }
}