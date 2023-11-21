﻿namespace IMAX.Application.Configuration.Tenant.Dto
{
    public class TimeScheduleCheckBillSettingsEditDto
    {
        public int HeadPeriodDay { get; set; }
        public int EndPeriodDay { get; set; }
        public int ElectricHeadPeriodDay { get; set; }
        public int ElectricEndPeriodDay { get; set; }
        public int ParkingCreateDay { get; set; }
        public int WaterHeadPeriodDay { get; set; }
        public int WaterEndPeriodDay { get; set; }
        public int BillNotificationTime1 { get; set; }
        public int BillNotificationTime2 { get; set; }
        public int BillNotificationTime3 { get; set; }
        public int BillDebtNotificationTime1 { get; set; }
        public int BillDebtNotificationTime2 { get; set; }
        public int BillDebtNotificationTime3 { get; set; }
    }
}
