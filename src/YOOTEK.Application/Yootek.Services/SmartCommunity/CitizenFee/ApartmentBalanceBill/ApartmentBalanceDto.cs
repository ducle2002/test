using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yootek.Common.Enum;
using YOOTEK.EntityDb;

namespace YOOTEK.Yootek.Services.SmartCommunity.CitizenFee.ApartmentBalanceBill
{
    [AutoMap(typeof(ApartmentBalance))]
    public class ApartmentBalanceDto : EntityDto<long>
    {
        public int? TenantId { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        [StringLength(1000)] public string CustomerName { get; set; }
        [StringLength(1000)] public string ApartmentCode { get; set; }
        public BillType? BillType { get; set; }
        public decimal Amount { get; set; }
        public long? CitizenTempId { get; set; }
        public long? UserBillId { get; set; }
        public DateTime CreationTime { get; set; }
        public EBalanceAction EBalanceAction { get; set; }
    }

    public class ApartmentBalanceDetailDto
    {
        public BillType? BillType { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
