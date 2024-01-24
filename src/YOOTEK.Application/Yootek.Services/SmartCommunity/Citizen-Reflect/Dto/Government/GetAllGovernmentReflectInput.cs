using Yootek.Common;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;

namespace Yootek.Services
{
    public enum ReflectFormId
    {
        GETALL = 1,
        NEW = 2,
        ASSIGNED = 3,
        HANDLING = 4,
        DECLINED = 5,
        ADMIN_CONFIRMED = 6,
        RATED = 7
    }

    public class GetAllGovernmentReflectInput : CommonInputDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public ReflectFormId FormId { get; set; }
        public int? State { get; set; }
        public long UserId { get; set; }
        public DateTime? FromDay { get; set; }
        public DateTime? ToDay { get; set; }
        public long? OrganizationUnitId { get; set; }
        public string Phone { get; set; }
        public string NameFeeder { get; set; }
    }

    public class ExportGovReflectInputDto
    {
        [CanBeNull] public List<long> Ids { get; set; }
        public ReflectFormId FormId { get; set; }
    }
}
