using Yootek.Common;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Yootek.Services.SmartSocial.Rates.Dto
{
    public class GetAllRateInputDto : CommonInputDto
    {
        public int? TenantId { get; set; }
        public long? ProviderId { get; set; }
        public long? ItemId { get; set; }
        public long? UserId { get; set; }
        public bool? IsComment { get; set; }
        public bool? IsMedia { get; set; }
        public OrderByRate? OrderBy { get; set; }
        public Rating? Rating { get; set; }
        public TypeOfRate? Type { get; set; }
        public long? OrderId { get; set; }
        public long? BookingId { get; set; }
    }
    public enum TypeOfRate
    {
        PROVIDER_DYNAMIC = 1,
        PROVIDER_STATIC = 2,
        ITEM = 3,
        OTHER = 4,
    }
    public enum OrderByRate
    {
        NEWEST = 1,
        LATEST = 2,
        RATING_ASC = 3,
        RATING_DESC = 4,
    }
    public enum Rating
    {
        ONE = 1,
        TWO = 2,
        THREE = 3,
        FOUR = 4,
        FIVE = 5,
    }
    public class CreateRateInputDto
    {
        public int? TenantId { get; set; }
        public long? ItemId { get; set; }
        public long? ProviderId { get; set; }
        public int? RatePoint { get; set; }
        public string FileUrl { get; set; }
        public int Type { get; set; }
        public string Comment { get; set; }
        [StringLength(256)]
        public string UserName { get; set; }
        [StringLength(100)]
        public string Email { get; set; }
        public long? AnswerRateId { get; set; }
        public long? OrderId { get; set; }
        public long? BookingId { get; set; }
    }
    public class CreateListRateInputDto
    {
        public List<CreateRateInputDto> Items { get; set; }
    }
    public class UpdateRateInputDto
    {
        public long Id { get; set; }
        public int? RatePoint { get; set; }
        public string FileUrl { get; set; }
        public string Comment { get; set; }
        [StringLength(256)]
        public string UserName { get; set; }
        [StringLength(100)]
        public string Email { get; set; }
        public long? AnswerRateId { get; set; }
    }
    public class UpdateRateByUserInputDto
    {
        public long Id { get; set; }
        public int? RatePoint { get; set; }
        public string FileUrl { get; set; }
        public string Comment { get; set; }
        [StringLength(256)]
        public string UserName { get; set; }
        [StringLength(100)]
        public string Email { get; set; }
        public long? AnswerRateId { get; set; }
    }
    public class DeleteRateInputDto
    {
        public long Id { get; set; }
    }
}
