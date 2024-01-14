using Yootek.EntityDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Application.Configuration.Tenant.Dto
{
    public class UserBillSettingsEditDto
    {
        public int DueDate { get; set; }
        public int DueMonth { get; set; }
        public int DueDateElectric { get; set; }
        public int DueMonthElectric { get; set; }
        public int DueDateWater { get; set; }
        public int DueMonthWater { get; set; }
        public int DueDateParking { get; set; }
        public int DueMonthParking { get; set; }
        public int DueDateLighting { get; set; }
        public int DueMonthLighting { get; set; }
        public int DueDateManager { get; set; }
        public int DueMonthManager { get; set; }
        public int DueDateResidence { get; set; }
        public int DueMonthResidence { get; set; }
        public int SendUserBillNotificationDay { get; set; }    
        public int ParkingBillType { get;set; }
    }
}
