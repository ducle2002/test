using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.EntityDb
{
    [Table("MaterialImportExports")]
    public class InventoryImportExport: FullAuditedEntity<long>, IMayHaveTenant
    {
        [StringLength(256)]
        public string Code { get; set; }
        public long? StaffId { get; set; }
        public DateTime? ImportExportDate { get; set; }
        public long? FromInventoryId { get; set; }
        public long? ToInventoryId { get; set; }
        public bool IsImport { get; set; }
        public int Amount { get; set; }
        public long MaterialId { get; set; }
        [StringLength(2000)]
        public string Description { get; set; }
        public int? TenantId { get; set; }
        public Guid Key { get; set; }
        public bool? IsApproved { get; set; }
    }
}
