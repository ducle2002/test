using System.Net.Http;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Runtime.Session;
using Yootek.App.ServiceHttpClient.Dto;
using Yootek.App.ServiceHttpClient.Dto.Social.Forum;
using Yootek.Common.Enum;

namespace Yootek.App.ServiceHttpClient.Social
{
    public interface IHttpSocialMediaService
    {
        #region iInvite
        Task<MicroserviceResultDto<PagedResultDto<InviteDto>>> GetListInvite(GetListInviteDto input);
        Task<MicroserviceResultDto<bool>> CreateInvite(CreateInviteDto input);
        Task<MicroserviceResultDto<bool>> UpdateInvite(UpdateInviteDto input);
        Task<MicroserviceResultDto<bool>> Delete(DeleteInviteDto input);
        #endregion
        #region iFanpage
        Task<MicroserviceResultDto<PagedResultDto<FanpageDto>>> GetListFanpageByUser(GetListFanpageByUserDto input);
        Task<MicroserviceResultDto<PagedResultDto<long>>> GetListMemberIdOfFanpage(GetListMemberOfFanpageDto input);
        Task<MicroserviceResultDto<bool>> CreateFanpage(CreateFanpageByAdminDto input);
        Task<MicroserviceResultDto<bool>> UpdateFanpage(UpdateFanpageByAdminDto input);
        Task<MicroserviceResultDto<bool>> UpdateMemberOfFanpage(UpdateMemberOfFanpageDto input);
        Task<MicroserviceResultDto<bool>> DeleteFanpage(DeleteFanpageDto input);
        Task<MicroserviceResultDto<bool>> DeleteMemberOfFanpage(DeleteMemberOfFanpageDto input);
        #endregion
        #region iGroup
        Task<MicroserviceResultDto<PagedResultDto<GroupDto>>> GetListGroupByUser(GetListGroupByUserDto input);
        Task<MicroserviceResultDto<PagedResultDto<GroupMemberDto>>> GetListMemberOfGroup(GetListMemberOfGroupDto input);
        Task<MicroserviceResultDto<GroupDto>> GetById(GetGroupByIdDto input);
        Task<MicroserviceResultDto<PagedResultDto<UserToGroupDto>>> GetListUser(GetSocialMediaUserDto input);
        Task<MicroserviceResultDto<bool>> CreateGroup(CreateGroupByAdminDto input);
        Task<MicroserviceResultDto<bool>> UpdateGroup(UpdateGroupByAdminDto input);
        Task<MicroserviceResultDto<bool>> UpdateMemberOfGroup(UpdateMemberOfGroupDto input);
        Task<MicroserviceResultDto<bool>> DeleteGroup(DeleteGroupByAdminDto input);
        Task<MicroserviceResultDto<bool>> DeleteMemberOfGroup(DeleteGroupMemberDto input);
        #endregion
        #region iPost
        Task<MicroserviceResultDto<PagedResultDto<PostDto>>> GetListPostByAdmin(GetListPostDto input);
        Task<MicroserviceResultDto<PagedResultDto<PostDto>>> GetListPost(GetListPostDto input);
        Task<MicroserviceResultDto<UserWallDto>> GetUserWall(GetUserWallDto input);
        Task<MicroserviceResultDto<PagedResultDto<PostDto>>> GetListSharedPost(GetListPostSharedDto input);
        Task<MicroserviceResultDto<PagedResultDto<PostDto>>> GetListPostOfFanpage(GetListPostOfFanpageDto input);
        Task<MicroserviceResultDto<PagedResultDto<PostDto>>> GetListPostOfGroup(GetListPostOfGroupDto input);
        Task<MicroserviceResultDto<bool>> CreatePost(CreatePostDto input);
        Task<MicroserviceResultDto<bool>> VerifyPost(VerifyPostDto input);
        Task<MicroserviceResultDto<bool>> UpdatePost(UpdatePostDto input);
        Task<MicroserviceResultDto<bool>> DeletePost(DeletePostDto input);
        #endregion
        #region iPostComment
        Task<MicroserviceResultDto<PagedResultDto<PostCommentDto>>> GetListComment (GetListCommentDto input);
        Task<MicroserviceResultDto<bool>> CreatePostComment(CreatePostCommentDto input);
        Task<MicroserviceResultDto<bool>> UpdatePostComment(UpdatePostCommentDto input);
        Task<MicroserviceResultDto<bool>> DeletePostComment(DeletePostCommentDto input);
        #endregion
        #region iPostReact
        Task<MicroserviceResultDto<PagedResultDto<PostReactDto>>> GetListReact(GetListPostReactDto input);
        Task<MicroserviceResultDto<bool>> CreatePostReact(CreatePostReactDto input);
        Task<MicroserviceResultDto<bool>> UpdatePostReact(UpdatePostReactDto input);
        Task<MicroserviceResultDto<bool>> DeletePostReact(DeletePostReactDto input);
        #endregion
        #region iBlackList
        Task<MicroserviceResultDto<PagedResultDto<BlackListDto>>> GetListBlackList(GetListBlackListDto input);
        Task<MicroserviceResultDto<bool>> CreateBlackList(CreateBlackListDto input);
        Task<MicroserviceResultDto<bool>> UpdateBlackList(UpdateBlackListDto input);
        Task<MicroserviceResultDto<bool>> DeleteBlackList(DeleteBlackListDto input);
        #endregion

        #region iReport

        Task<MicroserviceResultDto<PagedResultDto<ReportDto>>> GetListReport(GetListReportDto input);
        Task<MicroserviceResultDto<bool>> CreateReport(CreateReportDto input);

        #endregion
    }

    public class HttpSocialMediaService : IHttpSocialMediaService
    {
        private readonly HttpClient _client;
        private readonly IAbpSession _session;

        public HttpSocialMediaService(HttpClient client, IAbpSession session)
        {
            _client = client;
            _session = session;
        }

        #region Invite
        public async Task<MicroserviceResultDto<PagedResultDto<InviteDto>>> GetListInvite(GetListInviteDto input)
        {
            var query = "/api/Invite/GetListInvite" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);
            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<PagedResultDto<InviteDto>>>();
        }

        public async Task<MicroserviceResultDto<bool>> CreateInvite(CreateInviteDto input)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/Invite/CreateInvite");
            request.HandlePostAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }

        public async Task<MicroserviceResultDto<bool>> UpdateInvite(UpdateInviteDto input)
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/Invite/UpdateInvite");
            request.HandlePutAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }

        public async Task<MicroserviceResultDto<bool>> Delete(DeleteInviteDto input)
        {
            var query = "/api/Invite/Delete" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Delete, query);
            request.HandleDelete(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        
        #endregion

        #region Fanpage
        public async Task<MicroserviceResultDto<PagedResultDto<FanpageDto>>> GetListFanpageByUser(
            GetListFanpageByUserDto input)
        {
            var query = "/api/Fanpage/GetListFanpageByUser" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);
            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<PagedResultDto<FanpageDto>>>();
        }

        public async Task<MicroserviceResultDto<PagedResultDto<long>>> GetListMemberIdOfFanpage(
            GetListMemberOfFanpageDto input)
        {
            var query = "/api/Fanpage/GetListMemberOfFanpage" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);
            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<PagedResultDto<long>>>();
        }

        public async Task<MicroserviceResultDto<bool>> CreateFanpage(CreateFanpageByAdminDto input)
        {
            var query = "/api/Fanpage/CreateFanpage";
            using var request = new HttpRequestMessage(HttpMethod.Post, query);
            request.HandlePostAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }

        public async Task<MicroserviceResultDto<bool>> UpdateFanpage(UpdateFanpageByAdminDto input)
        {
            var query = "/api/Fanpage/UpdateFanpage";
            using var request = new HttpRequestMessage(HttpMethod.Put, query);
            request.HandlePutAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }

        public async Task<MicroserviceResultDto<bool>> UpdateMemberOfFanpage(UpdateMemberOfFanpageDto input)
        {
            var query = "/api/Fanpage/UpdateMemberOfFanpage";
            using var request = new HttpRequestMessage(HttpMethod.Put, query);
            request.HandlePutAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        
        public async Task<MicroserviceResultDto<bool>> DeleteFanpage(DeleteFanpageDto input)
        {
            var query = "/api/Fanpage/DeleteFanpage" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Delete, query);
            request.HandleDelete(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        
        public async Task<MicroserviceResultDto<bool>> DeleteMemberOfFanpage(DeleteMemberOfFanpageDto input)
        {
            var query = "/api/Fanpage/DeleteMemberOfFanpage" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Delete, query);
            request.HandleDelete(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        #endregion
        
        #region Group
        public async Task<MicroserviceResultDto<PagedResultDto<GroupDto>>> GetListGroupByUser(
            GetListGroupByUserDto input)
        {
            var query = "/api/Group/GetListGroupByUser" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);
            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<PagedResultDto<GroupDto>>>();
        }

        public async Task<MicroserviceResultDto<PagedResultDto<GroupMemberDto>>> GetListMemberOfGroup(
            GetListMemberOfGroupDto input)
        {
            var query = "/api/Group/GetListMemberOfGroup" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);
            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<PagedResultDto<GroupMemberDto>>>();
        }
        
        public async Task<MicroserviceResultDto<GroupDto>> GetById(GetGroupByIdDto input)
        {
            var query = "/api/Group/GetById" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);
            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            var result = await response.ReadContentAs<MicroserviceResultDto<GroupDto>>();
            return result;
        }

        public async Task<MicroserviceResultDto<PagedResultDto<UserToGroupDto>>> GetListUser(
            GetSocialMediaUserDto input)
        {
            var query = "/api/Group/GetListUser" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);
            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            var result = await response.ReadContentAs<MicroserviceResultDto<PagedResultDto<UserToGroupDto>>>();
            return result;
        }

        public async Task<MicroserviceResultDto<bool>> CreateGroup(CreateGroupByAdminDto input)
        {
            var query = "/api/Group/CreateGroup";
            using var request = new HttpRequestMessage(HttpMethod.Post, query);
            request.HandlePostAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }

        public async Task<MicroserviceResultDto<bool>> UpdateGroup(UpdateGroupByAdminDto input)
        {
            var query = "/api/Group/UpdateGroup";
            using var request = new HttpRequestMessage(HttpMethod.Put, query);
            request.HandlePutAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        
        public async Task<MicroserviceResultDto<bool>> UpdateMemberOfGroup(
            UpdateMemberOfGroupDto input)
        {
            var query = "/api/Group/UpdateMemberOfGroup";
            using var request = new HttpRequestMessage(HttpMethod.Put, query);
            request.HandlePutAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        
        public async Task<MicroserviceResultDto<bool>> DeleteGroup(DeleteGroupByAdminDto input)
        {
            var query = "/api/Group/DeleteGroup" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Delete, query);
            request.HandleDelete(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        
        public async Task<MicroserviceResultDto<bool>> DeleteMemberOfGroup(
            DeleteGroupMemberDto input)
        {
            var query = "/api/Group/DeleteMemberOfGroup" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Delete, query);
            request.HandleDelete(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        #endregion
        #region Post
        public async Task<MicroserviceResultDto<PagedResultDto<PostDto>>> GetListPostByAdmin(GetListPostDto input)
        {
            var query = "/api/Post/GetListPostByAdmin" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);
            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<PagedResultDto<PostDto>>>();
        }
        public async Task<MicroserviceResultDto<PagedResultDto<PostDto>>> GetListPost(GetListPostDto input)
        {
            var query = "/api/Post/GetListPost" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);
            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<PagedResultDto<PostDto>>>();
        }
        
        public async Task<MicroserviceResultDto<UserWallDto>> GetUserWall(GetUserWallDto input)
        {
            var query = "/api/Post/GetUserWall" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);
            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            var result = await response.ReadContentAs<MicroserviceResultDto<UserWallDto>>();
            return result;
        }

        public async Task<MicroserviceResultDto<PagedResultDto<PostDto>>> GetListSharedPost(GetListPostSharedDto input)
        {
            var query = "/api/Post/GetListSharedPost" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);
            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<PagedResultDto<PostDto>>>();
        }

        public async Task<MicroserviceResultDto<PagedResultDto<PostDto>>> GetListPostOfFanpage(GetListPostOfFanpageDto input)
        {
            var query = "/api/Post/GetListPostOfFanpage" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);
            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<PagedResultDto<PostDto>>>();
        }

        public async Task<MicroserviceResultDto<PagedResultDto<PostDto>>> GetListPostOfGroup(GetListPostOfGroupDto input)
        {
            var query = "/api/Post/GetListPostOfGroup" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);
            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<PagedResultDto<PostDto>>>();
        }

        public async Task<MicroserviceResultDto<bool>> CreatePost(CreatePostDto input)
        {
            var query = "/api/Post/CreatePost";
            using var request = new HttpRequestMessage(HttpMethod.Post, query);
            request.HandlePostAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        
        public async Task<MicroserviceResultDto<bool>> VerifyPost(VerifyPostDto input)
        {
            var query = "/api/Post/VerifyPost";
            using var request = new HttpRequestMessage(HttpMethod.Post, query);
            request.HandlePostAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();

        }

        public async Task<MicroserviceResultDto<bool>> UpdatePost(UpdatePostDto input)
        {
            var query = "/api/Post/UpdatePost";
            using var request = new HttpRequestMessage(HttpMethod.Put, query);
            request.HandlePutAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }

        public async Task<MicroserviceResultDto<bool>> DeletePost(DeletePostDto input)
        {
            var query = "/api/Post/DeletePost" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Delete, query);
            request.HandleDelete(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        #endregion
        #region postComment
        public async Task<MicroserviceResultDto<PagedResultDto<PostCommentDto>>> GetListComment(GetListCommentDto input)
        {
            var query = "/api/PostComment/GetListComment" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);
            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<PagedResultDto<PostCommentDto>>>();
        }
        public async Task<MicroserviceResultDto<bool>> CreatePostComment(CreatePostCommentDto input)
        {
            var query = "/api/PostComment/CreatePostComment";
            using var request = new HttpRequestMessage(HttpMethod.Post, query);
            request.HandlePostAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        public async Task<MicroserviceResultDto<bool>> UpdatePostComment(UpdatePostCommentDto input)
        {
            var query = "/api/PostComment/UpdatePostComment";
            using var request = new HttpRequestMessage(HttpMethod.Put, query);
            request.HandlePutAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        public async Task<MicroserviceResultDto<bool>> DeletePostComment(DeletePostCommentDto input)
        {
            var query = "/api/PostComment/DeletePostComment" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Delete, query);
            request.HandleDelete(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        #endregion
        #region postReact
        public async Task<MicroserviceResultDto<PagedResultDto<PostReactDto>>> GetListReact(GetListPostReactDto input)
        {
            var query = "/api/PostReact/GetListPostReact" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);
            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<PagedResultDto<PostReactDto>>>();
        }
        public async Task<MicroserviceResultDto<bool>> CreatePostReact(CreatePostReactDto input)
        {
            var query = "/api/PostReact/CreatePostReact";
            using var request = new HttpRequestMessage(HttpMethod.Post, query);
            request.HandlePostAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        public async Task<MicroserviceResultDto<bool>> UpdatePostReact(UpdatePostReactDto input)
        {
            var query = "/api/PostReact/UpdatePostReact";
            using var request = new HttpRequestMessage(HttpMethod.Put, query);
            request.HandlePutAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        public async Task<MicroserviceResultDto<bool>> DeletePostReact(DeletePostReactDto input)
        {
            var query = "/api/PostReact/DeletePostReact" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Delete, query);
            request.HandleDelete(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        #endregion

        #region blackList
        public async Task<MicroserviceResultDto<PagedResultDto<BlackListDto>>> GetListBlackList(GetListBlackListDto input)
        {
            var query = "/api/BlackList/GetListBlackList" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);
            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<PagedResultDto<BlackListDto>>>();
        }
        
        public async Task<MicroserviceResultDto<bool>> CreateBlackList(CreateBlackListDto input)
        {
            var query = "/api/BlackList/CreateBlackList";
            using var request = new HttpRequestMessage(HttpMethod.Post, query);
            request.HandlePostAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        
        public async Task<MicroserviceResultDto<bool>> UpdateBlackList(UpdateBlackListDto input)
        {
            var query = "/api/BlackList/UpdateBlackList";
            using var request = new HttpRequestMessage(HttpMethod.Put, query);
            request.HandlePutAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        
        public async Task<MicroserviceResultDto<bool>> DeleteBlackList(DeleteBlackListDto input)
        {
            var query = "/api/BlackList/DeleteBlackList" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Delete, query);
            request.HandleDelete(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        #endregion

        #region report

        public async Task<MicroserviceResultDto<PagedResultDto<ReportDto>>> GetListReport(GetListReportDto input)
        {
            var query = "/api/Report/GetListReport" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);
            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<PagedResultDto<ReportDto>>>();
        }
        
        public async Task<MicroserviceResultDto<bool>> CreateReport(CreateReportDto input)
        {
            var query = "/api/Report/CreateReport";
            using var request = new HttpRequestMessage(HttpMethod.Post, query);
            request.HandlePostAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }

        #endregion
    }
}