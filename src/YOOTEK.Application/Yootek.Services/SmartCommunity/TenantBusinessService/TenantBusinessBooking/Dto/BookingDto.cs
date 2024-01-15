using Abp.AutoMapper;
using Yootek.EntityDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Services
{
    [AutoMap(typeof(Booking))]
    public class BookingDto : Booking
    {
        public string ItemBookingName { get; set; }
        public string StoreName { get; set; }
        public string ItemBookingImageUrl { get; set; }
        public string RefuseReason { get; set; }
        public string CancelReason { get; set; }
        public DateTime? TimeRefuseOrCancel { get; set; }
        public Citizen CustomerInfo { get; set; }
    }

    [AutoMap(typeof(ItemBooking))]
    public class ItemBookingDto : ItemBooking
    {
    }

    [AutoMap(typeof(BookingHistory))]
    public class BookingHistoryDto : BookingHistory
    {
    }

    public class UpdateBookingStateInput
    {
        public StateBooking State { get; set; }
        public long BookingId { get; set; }

        public string RefuseReason { get; set; }
    }
}
