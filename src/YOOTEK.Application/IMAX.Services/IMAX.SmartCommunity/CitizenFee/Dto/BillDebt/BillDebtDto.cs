using Abp.AutoMapper;
using IMAX.EntityDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMAX.Services
{
    [AutoMap(typeof(BillDebt))]
    public class BillDebtDto: BillDebt
    {
    }
}
