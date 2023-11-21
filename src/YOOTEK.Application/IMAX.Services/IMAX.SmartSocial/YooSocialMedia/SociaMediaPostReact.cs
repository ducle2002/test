using System.Threading.Tasks;
using Abp;
using IMAX.App.ServiceHttpClient.Dto.Imax.Social.Forum;
using IMAX.App.ServiceHttpClient.Imax.Social;
using IMAX.Common.DataResult;

namespace IMAX.IMAX.Services.IMAX.SmartSocial.YooSocialMedia
{
    public interface ISocialMediaPostReact
    {
        Task<object> GetListReactAsync(GetListPostReactDto input);
        Task<object> CreatePostReactAsync(CreatePostReactDto input);
        Task<object> UpdatePostReactAsync(UpdatePostReactDto input);
        Task<object> DeletePostReactAsync(DeletePostReactDto input);
    }
    public class SocialMediaPostReactAppService : IMAXAppServiceBase, ISocialMediaPostReact
    {
        private readonly IHttpSocialMediaService _httpSocialMediaService;
        
        public SocialMediaPostReactAppService(IHttpSocialMediaService httpSocialMediaService)
        {
            _httpSocialMediaService = httpSocialMediaService;
        }
        public async Task<object> GetListReactAsync(GetListPostReactDto input)
        {
            try 
            {
                var getListResult = await _httpSocialMediaService.GetListReact(input);
                return DataResult.ResultSuccess(getListResult.Result.Items, getListResult.Message, getListResult.Result.TotalCount);
            }
            catch (System.Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> CreatePostReactAsync(CreatePostReactDto input)
        {
            try 
            {
                var createResult = await _httpSocialMediaService.CreatePostReact(input);
                return DataResult.ResultSuccess(createResult.Result, createResult.Message);
            }
            catch (System.Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> UpdatePostReactAsync(UpdatePostReactDto input)
        {
            try 
            {
                var updateResult = await _httpSocialMediaService.UpdatePostReact(input);
                return DataResult.ResultSuccess(updateResult.Result, updateResult.Message);
            }
            catch (System.Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> DeletePostReactAsync(DeletePostReactDto input)
        {
            try 
            {
                var deleteResult = await _httpSocialMediaService.DeletePostReact(input);
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