using Abp.AutoMapper;
using Yootek.EntityDb;
using System.Collections.Generic;

namespace Yootek.Services.Dto
{
    [AutoMap(typeof(Post))]
    public class PostDto : Post
    {
        public IEnumerable<PostComment> Comments { get; set; }
        public LikePost Like { get; set; }
        public int? CountComment { get; set; }
        public int? CountLike { get; set; }
        public string NameUserCreate { get; set; }
        public string ImageAvatarUserCreate { get; set; }
        public string FullName { get; set; }
    }


    [AutoMap(typeof(PostComment))]
    public class CommentPostDto : PostComment
    {
        public LikePost Like { get; set; }
        public int? CountLike { get; set; }
        public int? CountComments { get; set; }
        public string NameUserCreate { get; set; }
        public string ImageAvatarUserCreate { get; set; }
        public string FullName { get; set; }
    }
    
    public class ReportCommentDto : ReportComment
    {
        public string Comment { get; set; }
        public long? ParentCommentId { get; set; }
        public long? PostId { get; set; }
        public string NameUserCreate { get; set; }
        public string ImageAvatarUserCreate { get; set; }
        public string FullName { get; set; }
        public int? CountComments { get; set; }
    }
    

    [AutoMap(typeof(LikePost))]
    public class LikePostDto : LikePost
    {
    }
}
