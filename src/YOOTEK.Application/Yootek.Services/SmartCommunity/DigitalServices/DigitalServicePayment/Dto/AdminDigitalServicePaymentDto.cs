using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YOOTEK.EntityDb.IMAX.DichVu.DigitalServices;

namespace YOOTEK.Yootek.Services
{
    public class AdminDigitalServicePaymentDto: EntityDto<long>
    {
        public int? TenantId { get; set; }
        public long OrderId { get; set; }
        [StringLength(256)]
        public string ApartmentCode { get; set; }
        [StringLength(256)]
        public string Code { get; set; }
        public decimal Amount { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public long ServiceId { get; set; }
        public DigitalServicePaymentMethod Method { get; set; }
        public DigitalServicePaymentStatus Status { get; set; }
    }
}
