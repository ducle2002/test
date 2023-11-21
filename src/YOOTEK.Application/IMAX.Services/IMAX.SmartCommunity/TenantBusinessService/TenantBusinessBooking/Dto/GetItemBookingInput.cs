using IMAX.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMAX.Services
{
    public class GetItemBookingInput : CommonInputDto
    {
        public long StoreId { get; set; }
        public int Type { get; set; }
    }
}
