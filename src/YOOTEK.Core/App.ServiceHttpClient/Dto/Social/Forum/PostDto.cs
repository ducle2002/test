using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Yootek.Common;

namespace Yootek.App.ServiceHttpClient.Dto.Social.Forum
{
    #region enum
    public enum TypeOfPost
    {
        BelongedUser = 1,
        BelongedFanpage = 2,
        BelongedGroup = 3
    }
    public enum ReactState
    {
        None = -1,
        Like = 1,
        Favorite = 2,
        ThuongThuong =3,
        Haha = 4,
        Wow = 5,
        Sad = 6,
        Angry = 7,
    }
    public enum StateOfPost
    {
        NotVerified = 1,
        Verified = 2,
        Refused = 3
    }
    public enum EPostPrivacy
    {
        Public = 1,
        Private = 2,
        Friend = 3,
    }
    #endregion

    public class ShortenedUserDto
    {
        public long Id { get; set; }
        public string? FullName { get; set; }
        public string? UserName { get; set; }
        public string? ImageUrl { get; set; }
        public string? EmailAddress { get; set; }
    }
    
    public class PostDto : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public long? ForeignId { get; set; }
        public TypeOfPost Type { get; set; }
        public string? ContentPost { get; set; }
        public StateOfPost? State { get; set; }
        // public long? FeedbackId { get; set; }
        public List<long>? TagFriendIds { get; set; }
        public List<string>? ImageUrls { get; set; }
        public List<string>? VideoUrls { get; set; }
        public bool? IsShared { get; set; }
        public long? SharedPostId { get; set; }
        public EPostPrivacy? Privacy { get; set; }
        public int? BackGroundId { get; set; } = 0;
        public int? EmotionId { get; set; } = 0;
        
        public ShortenedUserDto? User { get; set; }
        public int? CountComment { get; set; }
        public int? CountReact { get; set; }
        public int? UserReact { get; set; }
        public GroupShortenedDto Group { get; set; }
        public FanpageShortenedDto FanPage { get; set; }
        public BlackListAction? BlackListAction { get; set; }
    }
    
    public class GroupShortenedDto
    {
        public string? Avatar { get; set; }
        public string Name { get; set; }
    }

    public class FanpageShortenedDto
    {
        public string? Avatar { get; set; }
        public string Name { get; set; }
    }
    
    public class UserInfo : ShortenedUserDto
    {
        public DateTime? Birthday { get; set; }
        public string? Address { get; set; }
    }
    public class UserWallDto
    {
        public UserInfo User { get; set; }
        // public List<PostDto>? Posts { get; set; }
        public long TotalPost { get; set; }
    }
    
    public class PostCommentDto
    {
        public long Id { get; set; }
        public int? TenantId { get; set; }
        public string Comment { get; set; }
        public long? ParentCommentId { get; set; }
        public long? CreatorUserId { get; set; }
        public long? PostId { get; set; }
        public DateTime CreationTime { get; set; }
        public ShortenedUserDto? User { get; set; }
        public long CountChildComment { get; set; }
        public long CountReact { get; set; }
        public int? UserReact { get; set; }
    }
    
    public class PostReactDto
    {
        public long Id { get; set; }
        public int? ReactState { get; set; }
        public long? CommentId { get; set; }
        public long? PostId { get; set; }
        public DateTime? CreationTime { get; set; }
        public long? CreatorUserId { get; set; }
        public DateTime? LastModificationTime { get; set; }
        public ShortenedUserDto? User { get; set; }
    }
    
    public class GetListPostDto: CommonInputDto
    {
        public long? UserId { get; set; }
        public TypeOfPost? TypeOfPost { get; set; }
        public StateOfPost? StateOfPost { get; set; }
        public EPostPrivacy? Privacy { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public long? ForeignId { get; set; }
        public EOrderByPost? OrderBy { get; set; }
    }
    
    public enum EOrderByPost
    {
        [YootekServiceBase.FieldName("CreationTime")]
        CreationTime = 1,
        [YootekServiceBase.FieldName("LastModificationTime")]
        LastModificationTime = 2
    }

    public class GetUserWallDto : CommonInputDto
    {
        public long UserId { get; set; }
    }
    
    public class GetListPostByAdminDto: CommonInputDto
    {
        public long? UserId { get; set; }
        public TypeOfPost? TypeOfPost { get; set; }
        public StateOfPost? StateOfPost { get; set; }
        public EPostPrivacy? Privacy { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public long? ForeignId { get; set; }
    }
    
    public class GetListPostOfFanpageDto : CommonInputDto
    {
        public long FanpageId { get; set; }
    }
    
    public class GetListPostOfGroupDto: CommonInputDto
    {
        public long GroupId { get; set; }
        public long? UserId { get; set; }
    }
    
    public class GetListPostSharedDto : CommonInputDto
    {
        public long SharedId { get; set; }
    }
    
    public class GetListPostReactDto : CommonInputDto
    {
        public long PostId { get; set; }
        public long? CommentId { get; set; }	
        public int? ReactState { get; set; }
    }
    
    public class GetListCommentDto: CommonInputDto
    {
        public long PostId { get; set; }
        public long? ParentCommentId { get; set; }
    }
    
    public class CreatePostDto
    {
        public long ForeignId { get; set; }
        public List<string>? ImageUrls { get; set; }
        public List<string>? VideoUrls { get; set; }
        public string? ContentPost { get; set; }
        public List<long>? TagFriendIds { get; set; }
        public int? Type { get; set; }
        public bool IsShared { get; set; } = false;
        public long? SharedPostId { get; set; }
        public EPostPrivacy? Privacy { get; set; }
        public int? BackgroundId { get; set; }
        public int? EmotionId { get; set; }
    }
    
    public class CreatePostReactDto
    {
        public int? ReactState { get; set; }
        public long? CommentId { get; set; }
        public long? PostId { get; set; }
    }
    
    public class CreatePostCommentDto
    {
        public string Comment { get; set; }
        public long? ParentCommentId { get; set; }
        public long? PostId { get; set; }
    }
    
    public class UpdatePostDto
    {
        public long Id { get; set; }
        public List<string>? ImageUrls { get; set; }
        public List<string>? VideoUrls { get; set; }
        public string? ContentPost { get; set; }
        public List<long>? TagFriendIds { get; set; }
        public StateOfPost? State { get; set; }
        public int? Type { get; set; }
        // public long? FeedbackId { get; set; }
        public EPostPrivacy? Privacy { get; set; }
        public int? BackgroundId { get; set; }
        public int? EmotionId { get; set; }
    }
    
    public class VerifyPostDto
    {
        public List<long> Ids { get; set; }
        public StateOfPost State { get; set; }
    }
    
    public class UpdatePostCommentDto
    {
        public long Id { get; set; }
        public string Comment { get; set; }
    }
    
    public class UpdatePostReactDto
    {
        public long PostId { get; set; }
        public long? CommentId { get; set; }
        public int? ReactState { get; set; }
    }
    
    public class DeletePostDto
    {
        public long Id { get; set; }
    }
    
    public class DeletePostReactDto
    {
        public long Id { get; set; }
    }
    
    public class DeletePostCommentDto
    {
        public long Id { get; set; }
    }
}