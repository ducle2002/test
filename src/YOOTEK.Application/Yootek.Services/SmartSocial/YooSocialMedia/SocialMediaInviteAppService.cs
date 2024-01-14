using System;
using System.Threading.Tasks;
using Abp;
using Yootek.App.ServiceHttpClient.Dto.Social.Forum;
using Yootek.App.ServiceHttpClient.Social;
using Yootek.Common.DataResult;

namespace Yootek.Yootek.Services.Yootek.SmartSocial.YooSocialMedia
{
    public interface ISocialMediaInvite
    {
        Task<object> CreateInviteAsync(CreateInviteDto input);
        Task<object> GetListInviteAsync(GetListInviteDto input);
        Task<object> UpdateInviteAsync(UpdateInviteDto input);
        Task<object> Delete(DeleteInviteDto input);
    }
    public class SocialMediaInviteAppService : YootekAppServiceBase, ISocialMediaInvite
    {
        
        private readonly IHttpSocialMediaService _httpSocialMediaService;
        
        public SocialMediaInviteAppService(IHttpSocialMediaService httpSocialMediaService)
        {
            _httpSocialMediaService = httpSocialMediaService;
        }
        
        public async Task<object> GetListInviteAsync(GetListInviteDto input)
        {
            try
            {
                var getListResult = await _httpSocialMediaService.GetListInvite(input);
                return DataResult.ResultSuccess(getListResult.Result.Items, getListResult.Message, getListResult.Result.TotalCount);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
        
        public async Task<object> CreateInviteAsync(CreateInviteDto input)
        {
            try
            {
                var createResult = await _httpSocialMediaService.CreateInvite(input);
                return DataResult.ResultSuccess(createResult.Result, createResult.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
        
        public async Task<object> UpdateInviteAsync(UpdateInviteDto input)
        {
            try
            {
                var updateResult = await _httpSocialMediaService.UpdateInvite(input);
                return DataResult.ResultSuccess(updateResult.Result, updateResult.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> Delete(DeleteInviteDto input)
        {
            try
            {
                var deleteResult = await _httpSocialMediaService.Delete(input);
                return DataResult.ResultSuccess(deleteResult.Result, deleteResult.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
    }
}