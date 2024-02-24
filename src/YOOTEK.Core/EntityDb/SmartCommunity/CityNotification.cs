
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Organizations;
using Yootek.Organizations.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.EntityDb
{
    public enum RECEIVE_TYPE
    {
        TENANT_ALL = 0,
        URBAN_ALL = 1,
        BUIDING_ALL = 2,
        APARTMENT = 3,
    }
    [Table("CityNotifications")]
    public class CityNotification : CreationAuditedEntity<long>, IMayHaveTenant, IDeletionAudited, IMayHaveOrganizationUnit, IMayHaveUrban, IMayHaveBuilding
    {
        [StringLength(2000)]
        public string Name { get; set; }
        public string Data { get; set; }
        [StringLength(2000)]
        public string FileUrl { get; set; }
        public int? Type { get; set; }
        public int? TenantId { get; set; }
        public long? DeleterUserId { get; set; }
        public DateTime? DeletionTime { get; set; }
        public bool IsDeleted { get; set; }
        public int? Follow { get; set; }
        public long? OrganizationUnitId { get; set; }
        public string ReceiverGroupCode { get; set; }
        public string DepartmentCode { get; set; }
        public string Department { get; set; }
        public int? State { get; set; }
        public bool? IsAllowComment { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public long[]? OrganizationUnitIds { get; set; }
        public RECEIVE_TYPE? ReceiveAll { get; set; }
        public bool? IsReceiveAll { get; set; }
        public string? AttachUrls { get; set; }
        
    }
}

