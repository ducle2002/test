

using Abp.Organizations;
using System;
using System.ComponentModel.DataAnnotations;

namespace Yootek.Organizations
{
    public enum APP_ORGANIZATION_TYPE
    {
        NONE = 10000,
        REPRESENTATIVE_NAME = 0,
        BUILDING = 1,
        URBAN = 2,
        CHAT = 3,
        FEEDBACK = 4,
        VOTE = 5,
        NOTIFICATION = 6,
        ADMINISTRATION = 7,
        BILL = 8,
        Q_A = 9,
        NEW = 10,
        HOTLINE = 11,
        MATERIAL = 12,
        UTILITIES = 13

    }

    public class AppOrganizationUnit : OrganizationUnit
    {

        [StringLength(256)]
        public string ProjectCode { get; set; }
        public APP_ORGANIZATION_TYPE Type { get; set; }
        public string Description { get; set; }
        [StringLength(1000)]
        public string ImageUrl { get; set; }

        [StringLength(10)]
        public string PhoneNumber { get;set; }

        [StringLength(50)]
        public string Email { get; set; }
        public decimal? Area { get; set; }

        [StringLength(250)]
        public string Address { get; set; }
        [StringLength(100)]
        public string BuildingType { get; set; }
        [StringLength(100)]
        public string ProvinceCode { get; set; }
        [StringLength(100)]
        public string DistrictCode { get; set; }

        [StringLength(100)]
        public string WardCode { get; set; }
        public int? NumberFloor { get; set; }    
        public bool IsManager { get; set; }
        public AppOrganizationUnit()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganizationUnit"/> class.
        /// </summary>
        /// <param name="tenantId">Tenant's Id or null for host.</param>
        /// <param name="displayName">Display name.</param>
        /// <param name="parentId">Parent's Id or null if OU is a root.</param>
        public AppOrganizationUnit(int? tenantId, string displayName, long? parentId = null, APP_ORGANIZATION_TYPE type = APP_ORGANIZATION_TYPE.REPRESENTATIVE_NAME, bool isManager = false)
        {
            TenantId = tenantId;
            DisplayName = displayName;
            ParentId = parentId;
            Type = type;
            IsManager = isManager;
        }
    }
}
