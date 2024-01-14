using System.Threading.Tasks;
using Abp;
using Yootek.App.ServiceHttpClient.Dto.Social.Forum;
using Yootek.App.ServiceHttpClient.Social;
using Yootek.Common.DataResult;

namespace Yootek.Yootek.Services.Yootek.SmartSocial.YooSocialMedia
{
    public interface ISocialMediaPost
    {
        Task<object> GetListPostByAdminAsync(GetListPostDto input);
        Task<object> GetListPostAsync(GetListPostDto input);
        Task<object> GetUserWallAsync(GetUserWallDto input);
        // Task<object> GetListSharedPostAsync(GetListPostSharedDto input);
        // Task<object> GetListPostOfFanpageAsync(GetListPostOfFanpageDto input);
        // Task<object> GetListPostOfGroupAsync(GetListPostOfGroupDto input);
        Task<object> CreatePostAsync(CreatePostDto input);
        Task<object> UpdatePostAsync(UpdatePostDto input);
        Task<object> DeletePostAsync(DeletePostDto input);   
    }
    public class SocialMediaPostAppService : YootekAppServiceBase, ISocialMediaPost
    {
        private readonly IHttpSocialMediaService _httpSocialMediaService;
        
        public SocialMediaPostAppService(IHttpSocialMediaService httpSocialMediaService)
        {
            _httpSocialMediaService = httpSocialMediaService;
        }

        public async Task<object> GetListPostByAdminAsync(GetListPostDto input)
        {
            try 
            {
                var getListResult = await _httpSocialMediaService.GetListPostByAdmin(input);
                return DataResult.ResultSuccess(getListResult.Result.Items, getListResult.Message, getListResult.Result.TotalCount);
            }
            catch (System.Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
        public async Task<object> GetListPostAsync(GetListPostDto input)
        {
            try 
            {
                var getListResult = await _httpSocialMediaService.GetListPost(input);
                return DataResult.ResultSuccess(getListResult.Result.Items, getListResult.Message, getListResult.Result.TotalCount);
            }
            catch (System.Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
        
        public async Task<object> GetUserWallAsync(GetUserWallDto input)
        {
            try 
            {
                var getListResult = await _httpSocialMediaService.GetUserWall(input);
                return DataResult.ResultSuccess(getListResult.Result, getListResult.Message);
            }
            catch (System.Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        // public async Task<object> GetListSharedPostAsync(GetListPostSharedDto input)
        // {
        //     try 
        //     {
        //         var getListResult = await _httpForumService.GetListSharedPost(input);
        //         return DataResult.ResultSuccess(getListResult.Result, getListResult.Message);
        //     }
        //     catch (System.Exception e)
        //     {
        //         Logger.Fatal(e.Message);
        //         throw new AbpException(e.Message);
        //     }
        // }

        // public async Task<object> GetListPostOfFanpageAsync(GetListPostOfFanpageDto input)
        // {
        //     try 
        //     {
        //         var getListResult = await _httpForumService.GetListPostOfFanpage(input);
        //         return DataResult.ResultSuccess(getListResult.Result, getListResult.Message);
        //     }
        //     catch (System.Exception e)
        //     {
        //         Logger.Fatal(e.Message);
        //         throw new AbpException(e.Message);
        //     }
        // }
        //
        // public async Task<object> GetListPostOfGroupAsync(GetListPostOfGroupDto input)
        // {
        //     try 
        //     {
        //         var getListResult = await _httpForumService.GetListPostOfGroup(input);
        //         return DataResult.ResultSuccess(getListResult.Result, getListResult.Message);
        //     }
        //     catch (System.Exception e)
        //     {
        //         Logger.Fatal(e.Message);
        //         throw new AbpException(e.Message);
        //     }
        // }

        public async Task<object> CreatePostAsync(CreatePostDto input)
        {
            try 
            {
                var createResult = await _httpSocialMediaService.CreatePost(input);
                return DataResult.ResultSuccess(createResult.Result, createResult.Message);
            }
            catch (System.Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
        
        public async Task<object> VerifyPostAsync(VerifyPostDto input)
        {
            try 
            {
                var updateResult = await _httpSocialMediaService.VerifyPost(input);
                return DataResult.ResultSuccess(updateResult.Result, updateResult.Message);
            }
            catch (System.Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> UpdatePostAsync(UpdatePostDto input)
        {
            try 
            {
                var updateResult = await _httpSocialMediaService.UpdatePost(input);
                return DataResult.ResultSuccess(updateResult.Result, updateResult.Message);
            }
            catch (System.Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> DeletePostAsync(DeletePostDto input)
        {
            try 
            {
                var deleteResult = await _httpSocialMediaService.DeletePost(input);
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