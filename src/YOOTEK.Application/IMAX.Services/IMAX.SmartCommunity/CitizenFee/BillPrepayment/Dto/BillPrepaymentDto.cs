using AutoMapper;
using IMAX.Common;
using IMAX.EntityDb;
using System.Collections.Generic;

namespace IMAX.Services
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
