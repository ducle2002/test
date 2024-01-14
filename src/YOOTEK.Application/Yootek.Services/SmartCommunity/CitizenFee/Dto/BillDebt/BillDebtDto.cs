using Abp.AutoMapper;
using Yootek.EntityDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Services
{
    [AutoMap(typeof(BillDebt))]
    public class BillDebtDto: BillDebt
    {
    }
}
