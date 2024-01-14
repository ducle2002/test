using System;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using JetBrains.Annotations;

namespace Yootek.EntityDb
{
    public enum GuestVehicle
    {
        Other = 0,
        Car = 1,
        Motorcycle = 2,
        Bicycle = 3,
        PublicTransportation = 4,
    }

    public enum GuestStatus
    {
        Other = 0,
        Pending = 1,
        Approved = 2,
        Rejected = 3,
        CheckedIn = 4,
        CheckedOut = 5,
    }

    [Table("Guests")]
    public class Guest : FullAuditedEntity<long>, IMustHaveTenant
    {
        public long GuestFormId { get; set; }
        public int TenantId { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        [CanBeNull] public string Address { get; set; }
        [CanBeNull] public string IdNumber { get; set; }
        [CanBeNull] public string IdCardFrontImageUrl { get; set; }
        [CanBeNull] public string IdCardBackImageUrl { get; set; }
        public DateTime CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        [CanBeNull] public string PlaceDetail { get; set; }
        public GuestStatus Status { get; set; }
        public GuestVehicle? Vehicle { get; set; }
        [CanBeNull] public string VehicleNumber { get; set; }
        [CanBeNull] public string HostName { get; set; }
        [CanBeNull] public string HostPhoneNumber { get; set; }
        [CanBeNull] public string Description { get; set; }
    }
}