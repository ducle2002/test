using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Organizations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.EntityDb
{
    public enum TypeBookingEnum
    {
        Form = 1,
        Item = 2
    }

    public enum StateBooking
    {
        Requesting = 1,
        Accepted = 2,
        Denied = 3,
        Cancel = 4,
        Expired = 5,
        Completed = 6
    }

    public class Booking : Entity<long>, IHasCreationTime, ICreationAudited, IMayHaveTenant
    {
        public TypeBookingEnum? TypeBooking { get; set; }
        public long StoreId { get; set; }
        [StringLength(100)]
        public string BookingCode { get; set; }
        public StateBooking State { get; set; }
        [StringLength(100)]
        public string CustomerName { get; set; }
        [StringLength(200)]
        public string CustomerAddress { get; set; }
        [StringLength(100)]
        public string CustomerPhoneNumber { get; set; }
        public int Amount { get; set; }
        public string CustomerComment { get; set; }
        public long? ItemBookingId { get; set; }
        [StringLength(2000)]
        public string BookingTime { get; set; }
        public DateTime? BookingDay { get; set; }
        public int? TenantId { get; set; }
        public DateTime CreationTime { get; set; }
        public long? CreatorUserId { get; set; }
        public long? OrganizationUnitId { get; set; }
    }

    [Table("BookingHistories")]
    public class BookingHistory : Entity<long>, IMayHaveTenant, ICreationAudited, IHasCreationTime
    {
        public StateBooking State { get; set; }
        public long BookingId { get; set; }
        [StringLength(1000)]
        public string RefuseReason { get; set; }
        [StringLength(1000)]
        public string CancelReason { get; set; }
        public int? TenantId { get; set; }
        public long? CreatorUserId { get; set; }
        public DateTime CreationTime { get; set; }
    }

    public class ItemBooking : Entity<long>, IMayHaveTenant
    {
        //public TypeBookingEnum? TypeBooking { get; set; }
        [StringLength(200)]
        public string Name { get; set; }
        [StringLength(500)]
        public string ImageUrl { get; set; }
        [StringLength(2000)]
        public string OpenTimes { get; set; }
        [StringLength(2000)]
        public string Description { get; set; }
        public int? Price { get; set; }
        public bool? IsFree { get; set; }
        public UnitItemBooking Unit { get; set; }
        public long StoreId { get; set; }
        public int? TenantId { get; set; }
    }

    public enum UnitItemBooking
    {
        OneHour = 1,
        HalfHour = 2,
        OneDay = 3,
        HalfDay = 4
    }
}
