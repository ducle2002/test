using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yootek.EntityDb;

namespace Yootek.Services
{
    public class SendUserBillNotificationInput
    {
        public string ApartmentCode { get; set; }
        public DateTime Period { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public UserBillStatus? Status { get; set; }
    }
}
