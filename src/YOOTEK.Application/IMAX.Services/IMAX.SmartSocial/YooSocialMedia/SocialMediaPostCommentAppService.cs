using System.Threading.Tasks;
using Abp;
using IMAX.App.ServiceHttpClient.Dto.Imax.Social.Forum;
using IMAX.App.ServiceHttpClient.Imax.Social;
using IMAX.Common.DataResult;

namespace IMAX.IMAX.Services.IMAX.SmartSocial.YooSocialMedia
{
    public interface ISocialMediaPostComment
    {
        Task<object> GetListCommentAsync(GetListCommentDto input);
        Task<object> CreateCommentAsync(CreatePostCommentDto input);
        Task<object> UpdateCommentAsync(UpdatePostCommentDto input);
        Task<object> DeleteCommentAsync(DeletePostCommentDto input);
    }
    public class SocialMediaPostCommentAppService : IMAXAppServiceBase, ISocialMediaPostComment
    {
        private readonly IHttpSocialMediaService _httpSocialMediaService;

        public SocialMediaPostCommentAppService(IHttpSocialMediaService httpSocialMediaService)
        {
            _httpSocialMediaService = httpSocialMediaService;
        }

        public async Task<object> GetListCommentAsync(GetListCommentDto input)
        {
            try 
            {
                var getListResult = await _httpSocialMediaService.GetListComment(input);
                return DataResult.ResultSuccess(getListResult.Result.Items, getListResult.Message, getListResult.Result.TotalCount);
            }
            catch (System.Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> CreateCommentAsync(CreatePostCommentDto input)
        {
            try 
            {
                var createResult = await _httpSocialMediaService.CreatePostComment(input);
                return DataResult.ResultSuccess(createResult.Result, createResult.Message);
            }
            catch (System.Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> UpdateCommentAsync(UpdatePostCommentDto input)
        {
            try 
            {
                var updateResult = await _httpSocialMediaService.UpdatePostComment(input);
                return DataResult.ResultSuccess(updateResult.Result, updateResult.Message);
            }
            catch (System.Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> DeleteCommentAsync(DeletePostCommentDto input)
        {
            try 
            {
                var deleteResult = await _httpSocialMediaService.DeletePostComment(input);
                return DataResult.ResultSuccess(deleteResult.Result, deleteResult.Message);
            }
            catch (System.Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
    }
}