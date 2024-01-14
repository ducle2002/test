using Abp.Authorization.Users;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Organizations;
using Yootek.Authorization.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.EntityDb
{
    public enum STATE_STAFF
    {
        NEW = 0,
        ACCEPTED = 1, //da xac minh
        MATCHCHECK = 3,
        MISMATCH = 4,
        REFUSE = 5,
        DISABLE = 6,
        EDITED = 7,
    }
    public enum GET_STAFF_FORMID
    {
        GET_ALL = 1,
        GET_VERIFIED = 2,
        GET_MATCH = 3,
        GET_REFUSE = 4,
        GET_UNVERIFIED = 5,
        GET_DISABLE = 6,
        GET_NEW = 7
    }
    [Table("Staff")]
    public class Staff : FullAuditedEntity<long>, IMayHaveTenant, IMayHaveOrganizationUnit
    {
        [StringLength(1000)]
        public string Specialize { get; set; }
        public int? TenantId { get; set; }
        public int PositionId { get; set; }
        public long? OrganizationUnitId { get; set; }

        public long UserId { get; set; }
        public long? DepartmentUnitId { get; set; }
        public int? Type { get; set; }

        [StringLength(50)]
        public string? Name { get; set; }
        [StringLength(50)]
        public string? Surname { get; set; }
        [StringLength(256)]
        public string? AccountName { get; set; }
        [StringLength(128)]
        public string? Password { get; set; }
        [StringLength(256)]
        public string? Email { get; set; }
        public DateTime? DateOfBirth { get; set; }
        [StringLength(1000)]
        public string? AddressOfBirth { get; set; }
        //public string? PositionName { get; set; }


    }
}
