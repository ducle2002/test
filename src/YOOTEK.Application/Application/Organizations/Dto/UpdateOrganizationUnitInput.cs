using System;
using System.ComponentModel.DataAnnotations;
using Abp.Organizations;

namespace IMAX.Organizations.Dto
{
    public class UpdateOrganizationUnitInput
    {
        [Range(1, long.MaxValue)]
        public long Id { get; set; }
        [Required]
        [StringLength(OrganizationUnit.MaxDisplayNameLength)]
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public bool IsManager { get; set; }
        public string ProjectCode { get; set; }
        public Guid? GroupdId { get; set; } 
        public APP_ORGANIZATION_TYPE[] Types { get; set; }
    }
}