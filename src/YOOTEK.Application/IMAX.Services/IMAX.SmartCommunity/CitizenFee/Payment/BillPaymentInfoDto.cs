using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using IMAX.Common.Enum;
using IMAX.EntityDb;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMAX.IMAX.Services.IMAX.SmartCommunity.CitizenFee.Dto
{
    public class AdminUserBillPaymentOutputDto : UserBillPayment
    {
        [NotMapped] public string FullName { get; set; }
        public long[] BillIds { get; set; }
        public List<BillPaidDto> BillList { get; set; }
        public List<BillPaidDto> BillListDebt { get; set; }
        public List<BillPaidDto> BillListPrepayment { get; set; }
        public List<BillDebt> DebtList { get; set; }
        public double? TotalPayment { get; set; }
    }

    [AutoMap(typeof(UserBill))]
    public class BillPaidDto: EntityDto<long>
    {
        public string Code { get; set; }
        public DateTime? Period { get; set; }
        public string Title { get; set; }
        public string ApartmentCode { get; set; }
        public BillType BillType { get; set; }
        public UserBillStatus Status { get; set; }
        public double? LastCost { get; set; }
        public decimal? IndexEndPeriod { get; set; }
        public decimal? IndexHeadPeriod { get; set; }
        public decimal? TotalIndex { get; set; }
        public double? PayAmount { get; set; }
        public double Amount { get; set; }
        public decimal? DebtTotal { get; set; }
    }

    public class BillPaymentInfo
    {
        public List<BillPaidDto> BillList { get; set; }
        public List<BillPaidDto> BillListDebt { get; set; }
        public List<BillPaidDto> BillListPrepayment { get; set; }
    } 

}
