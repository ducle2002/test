using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yootek.Organizations.Interface;

namespace YOOTEK.EntityDb.IMAX.DichVu.DigitalServices
{
    public class DigitalServicePayment : FullAuditedEntity<long>, IMayHaveTenant, IMayHaveBuilding, IMayHaveUrban
    {
        public int? TenantId { get; set; }
        public long OrderId { get; set; }
        /// <summary>
        /// Mã căn hộ
        /// </summary>
        [StringLength(256)]
        public string ApartmentCode { get; set; }
        /// <summary>
        /// Mã thanh toán, ngẫu nhiên
        /// </summary>
        [StringLength(256)]
        public string Code { get; set; }
        /// <summary>
        /// Số tiền thanh toán
        /// </summary>
        public decimal Amount { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        /// <summary>
        /// Ghi chú thanh toán
        /// </summary>
        [StringLength(2000)]
        public string Note { get; set; }
        /// <summary>
        /// Các thuộc tính động
        /// </summary>
        public string Properties { get; set; }
        /// <summary>
        /// Dịch vụ
        /// </summary>
        public long ServiceId { get;set; }
        public DigitalServicePaymentMethod Method { get;set; }
        /// <summary>
        /// Trạng thái thanh toán
        /// </summary>
        public DigitalServicePaymentStatus Status { get; set; }

    }

    public enum DigitalServicePaymentMethod
    {
        DIRECT = 1,
        MOMO = 2,
        ONEPAY = 3,
        BANKING = 6
    }

    public enum DigitalServicePaymentStatus
    {
        PENDING = 1,
        SUCCESS = 2,
        FAIL = 3
    }
}
