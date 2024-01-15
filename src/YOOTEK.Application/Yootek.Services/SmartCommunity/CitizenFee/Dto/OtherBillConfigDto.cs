using Yootek.EntityDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Services
{
    public class OtherBillConfigDto
    {
        public string Key { get; set; }
        public int Value { get; set; } = 7;
        public long Id { get; set; }
        public double? Price { get; set; }
        public string Properties { get; set; }
        public BillConfigPricesType? PricesType { get; set; }
    }
}
