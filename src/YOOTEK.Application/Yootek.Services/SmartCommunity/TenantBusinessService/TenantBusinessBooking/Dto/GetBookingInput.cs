using Yootek.Common;
using Yootek.EntityDb;
using System;

namespace Yootek.Services
{
    public enum FormGetBooing
    {
        //admin get
        GET_ALL = 1,
        NEW = 2,
        ACCEPTED = 3,
        REFUSE = 4,
        CANCEL = 5,
        EXPIRED = 6,
        COMPLETED = 7,

        //user get
        USER_GETALL = 11,
        USER_GET_REQUESTING = 12,
        USER_GET_ACCEPTED = 13,
        USER_GET_COMPLETED = 14,
        USER_GET_CANCEL = 15
    }

    public enum FormTimeSelect
    {
        TODAY = 1,
        TOMORROW = 2,
        THISWEEK = 3,
        THISMONTH = 4,
        ALL = 5
    }

    public class GetBookingInput : CommonInputDto
    {
        public long StoreId { get; set; }
        public FormTimeSelect? TimeSelect { get; set; }
        public FormGetBooing? FormId { get; set; }
        public DateTime? FromTime { get; set; }
        public DateTime? ToTime { get; set; }
        public long? ItemBookingId { get; set; }

        public TypeBookingEnum? TypeBooking { get; set; }
    }

    public class GetTimeBookedByItemInput
    {
        public long ItemBookingId { get; set; }
        public DateTime BookingDay { get; set; }
    }
}
