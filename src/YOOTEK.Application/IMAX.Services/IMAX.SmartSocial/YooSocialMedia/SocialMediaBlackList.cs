using System;
using System.Threading.Tasks;
using Abp;
using IMAX.App.ServiceHttpClient.Dto.Imax.Social.Forum;
using IMAX.App.ServiceHttpClient.Imax.Social;
using IMAX.Common.DataResult;

namespace IMAX.IMAX.Services.IMAX.SmartSocial.YooSocialMedia
{
    public interface ISocialMediaBlackList
    {
        Task<object> GetAllBlackListAsync(GetListBlackListDto input);
        Task<object> CreateBlackListAsync(CreateBlackListDto input);
        Task<object> UpdateBlackListAsync(UpdateBlackListDto input);
        Task<object> DeleteBlackListAsync(DeleteBlackListDto input);
    }
    public class SocialMediaBlackListAppService : IMAXAppServiceBase, ISocialMediaBlackList
    {
        private readonly IHttpSocialMediaService _httpSocialMediaService;
        
        public SocialMediaBlackListAppService(IHttpSocialMediaService httpSocialMediaService)
        {
            _httpSocialMediaService = httpSocialMediaService;
        }
        
        public async Task<object> GetAllBlackListAsync(GetListBlackListDto input)
        {
            try
            {
                var getListResult = await _httpSocialMediaService.GetListBlackList(input);
                return DataResult.ResultSuccess(getListResult.Result.Items, getListResult.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> CreateBlackListAsync(CreateBlackListDto input)
        {
            try
            {
                var getListResult = await _httpSocialMediaService.CreateBlackList(input);
                return DataResult.ResultSuccess(getListResult.Result, getListResult.Message);
            }   
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> UpdateBlackListAsync(UpdateBlackListDto input)
        {
            try
            {
                var getListResult = await _httpSocialMediaService.UpdateBlackList(input);
                return DataResult.ResultSuccess(getListResult.Result, getListResult.Message);
            }   
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> DeleteBlackListAsync(DeleteBlackListDto input)
        {
            try
            {
                var getListResult = await _httpSocialMediaService.DeleteBlackList(input);
                return DataResult.ResultSuccess(getListResult.Result, getListResult.Message);
            }   
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
    }
}