using System;
using Abp.AutoMapper;
using JetBrains.Annotations;
using Yootek.Common;
using Yootek.EntityDb;
using static Yootek.YootekServiceBase;

namespace Yootek.Yootek.Services.Yootek.SmartCommunity.Guests.Dto
{
    public enum GetListGuestOrderBy
    {
        [FieldName("CreationTime")] CreationTime = 1,
        [FieldName("CheckInTime")] CheckInTime = 2,
        [FieldName("CheckOutTime")] CheckOutTime = 3,
        [FieldName("Name")] Name = 4,
    }

    public class GetListGuestDto : CommonInputDto
    {
        public long? UrbanId { get; set; }
        public GuestStatus? Status { get; set; }
        public GetListGuestOrderBy? OrderBy { get; set; }
        public DateTime? CheckInTimeFrom { get; set; }
        public DateTime? CheckInTimeTo { get; set; }
        public DateTime? CheckOutTimeFrom { get; set; }
        public DateTime? CheckOutTimeTo { get; set; }
        public DateTime? CreationDate { get; set; }
        public DateTime? CreationTimeFrom { get; set; }
        public DateTime? CreationTimeTo { get; set; }
    }

    public class GetDetailGuestDto
    {
        public long Id { get; set; }
    }

    public class BasicGuestDto
    {
        public long Id { get; set; }
        public long GuestFormId { get; set; }
        public string GuestFormName { get; set; }
        public int TenantId { get; set; }
        public long UrbanId { get; set; }
        public string UrbanName { get; set; }
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
        public DateTime CreationTime { get; set; }
    }

    public class DetailGuestDo : Guest
    {
        public string GuestFormName { get; set; }
        public long UrbanId { get; set; }
        public string UrbanName { get; set; }
    }

    [AutoMap(typeof(Guest))]
    public class UserSubmitFormDto
    {
        public long GuestFormId { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        [CanBeNull] public string Address { get; set; }
        [CanBeNull] public string IdNumber { get; set; }
        [CanBeNull] public string IdCardFrontImageUrl { get; set; }
        [CanBeNull] public string IdCardBackImageUrl { get; set; }
        public DateTime CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public GuestVehicle? Vehicle { get; set; }
        [CanBeNull] public string VehicleNumber { get; set; }
        [CanBeNull] public string PlaceDetail { get; set; }
        [CanBeNull] public string HostName { get; set; }
        [CanBeNull] public string HostPhoneNumber { get; set; }
        [CanBeNull] public string Description { get; set; }
    }

    public class AdminChangeStatusDto
    {
        public long Id { get; set; }
        public GuestStatus Status { get; set; }
    }
}