using System;
using Abp.AutoMapper;
using Yootek.Common;
using Yootek.EntityDb;
using JetBrains.Annotations;
using static Yootek.YootekServiceBase;

namespace Yootek.Yootek.Services.Yootek.SmartCommunity.Guests.Dto
{
    public enum GetListGuestFormOrderBy
    {
        [FieldName("CreationTime")] CreationTime = 1,
        [FieldName("Name")] Name = 2,
    }
    
    public class GetListGuestFormDto : CommonInputDto
    {
        public long? UrbanId { get; set; }
        public GetListGuestFormOrderBy? OrderBy { get; set; }
    }

    [AutoMap(typeof(GuestForm))]
    public class CreateGuestFormDto
    {
        public string Name { get; set; }
        public long UrbanId { get; set; }
        [CanBeNull] public string ImageUrl { get; set; }
        [CanBeNull] public string PhoneNumber { get; set; }
        [CanBeNull] public string Address { get; set; }
        [CanBeNull] public string Description { get; set; }
    }

    public class BasicGuestFormDto
    {
        public long Id { get; set; }
        public int TenantId { get; set; }
        public string Name { get; set; }
        public long UrbanId { get; set; }
        [CanBeNull] public string UrbanName { get; set; }
        [CanBeNull] public string ImageUrl { get; set; }
        [CanBeNull] public string PhoneNumber { get; set; }
        [CanBeNull] public string Address { get; set; }
        [CanBeNull] public string Description { get; set; }
        public DateTime CreationTime { get; set; }
    }

    public class DetailGuestFormDto : GuestForm
    {
        [CanBeNull] public string UrbanName { get; set; }
    }

    [AutoMap(typeof(GuestForm))]
    public class UpdateGuestFormDto
    {
        public long Id { get; set; }
        [CanBeNull] public string Name { get; set; }
        [CanBeNull] public string PhoneNumber { get; set; }
        [CanBeNull] public string Address { get; set; }
        [CanBeNull] public string Description { get; set; }
        [CanBeNull] public string ImageUrl { get; set; }
    }
}