using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yootek.Common.Enum;

namespace YOOTEK.EntityDb
{
    [Table("EPaymentBalanceTenants")]
    public class EPaymentBalanceTenant : FullAuditedEntity<long>
    {
        [StringLength(1000)]
        public string Title { get; set; }
        public long? BillPaymentId { get; set; }
        public long EPaymentId { get; set; }
        public double BalanceRemaining { get; set; }
        public UserBillPaymentMethod Method { get; set; }
        public EBalanceAction EBalanceAction { get; set; }
        public int? TenantId { get; set; }       
        public EbalancePaymentType? EbalancePaymentType { get; set; }
    }

    public enum EBalanceAction
    {
        Add = 1,
        Sub = 2
    }

    public enum EbalancePaymentType
    {
        UserBill = 0,
        DigitalService = 1
    }
}
