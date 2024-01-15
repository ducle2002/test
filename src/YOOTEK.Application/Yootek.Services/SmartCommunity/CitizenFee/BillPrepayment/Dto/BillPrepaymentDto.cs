using AutoMapper;
using Yootek.Common;
using Yootek.EntityDb;
using System.Collections.Generic;

namespace Yootek.Services
{
    public class GetBillPrepaymentInput : CommonInputDto
    {
    }

    [AutoMap(typeof(BillPrepayment))]
    public class BillPrepaymentDto : BillPrepayment
    {
        public string BillTypeName { get; set; }
        public List<long> Formulas { get; set; }
    }

}
