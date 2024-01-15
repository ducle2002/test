using System.Collections.Generic;
using Abp.Domain.Entities;
using Yootek.Common;

namespace Yootek.Yootek.Services.Yootek.Payments.Dto
{
    public class GetListPaymentDto
    {
        public int? Method { get; set; }
        public int? Status { get; set; }
        public int? Type { get; set; }
        public int? TransactionId { get; set; }
        public int? MaxResultCount { get; set; } = 10;
        public int? SkipCount { get; set; } = 0;
    }

    public class GetListPaymentListResultDto
    {
        public int TotalCount { get; set; }
        public List<object> Items { get; set; }
    }
}