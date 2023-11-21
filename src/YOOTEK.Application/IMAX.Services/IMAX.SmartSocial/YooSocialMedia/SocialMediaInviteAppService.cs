using System;
using System.Threading.Tasks;
using Abp;
using IMAX.App.ServiceHttpClient.Dto.Imax.Social.Forum;
using IMAX.App.ServiceHttpClient.Imax.Social;
using IMAX.Common.DataResult;

namespace IMAX.IMAX.Services.IMAX.SmartSocial.YooSocialMedia
{
    public interface ISocialMediaInvite
    {
        Task<object> CreateInviteAsync(CreateInviteDto input);
        Task<object> GetListInviteAsync(GetListInviteDto input);
        Task<object> UpdateInviteAsync(UpdateInviteDto input);
    }
    public class SocialMediaInviteAppService : IMAXAppServiceBase, ISocialMediaInvite
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
    }
}