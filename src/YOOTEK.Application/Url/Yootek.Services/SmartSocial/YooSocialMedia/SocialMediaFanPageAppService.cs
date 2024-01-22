using System;
using System.Threading.Tasks;
using Abp;
using Yootek.App.ServiceHttpClient.Dto.Social.Forum;
using Yootek.App.ServiceHttpClient.Social;
using Yootek.Common.DataResult;

namespace Yootek.Yootek.Services.Yootek.SmartSocial.YooSocialMedia
{
    public interface ISocialMediaFanPage
    {
        Task<object> GetListFanpageByUserAsync(GetListFanpageByUserDto input);
        Task<object> GetListMemberOfFanpageAsync(GetListMemberOfFanpageDto input);
        Task<object> CreateFanpageAsync(CreateFanpageByAdminDto input);
        Task<object> UpdateFanpageAsync(UpdateFanpageByAdminDto input);
        Task<object> UpdateMemberOfFanpageAsync(UpdateMemberOfFanpageDto input);
        Task<object> DeleteFanpageAsync(DeleteFanpageDto input);
        Task<object> DeleteMemberOfFanpageAsync(DeleteMemberOfFanpageDto input);
    }
    public class SocialMediaFanPageAppService : YootekAppServiceBase, ISocialMediaFanPage
    {
        
        private readonly IHttpSocialMediaService _httpSocialMediaService;
        
        public SocialMediaFanPageAppService(IHttpSocialMediaService httpSocialMediaService)
        {
            _httpSocialMediaService = httpSocialMediaService;
        }

        public async Task<object> GetListFanpageByUserAsync(GetListFanpageByUserDto input)
        {
            try
            {
                var getListResult = await _httpSocialMediaService.GetListFanpageByUser(input);
                return DataResult.ResultSuccess(getListResult.Result.Items, getListResult.Message, getListResult.Result.TotalCount);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> GetListMemberOfFanpageAsync(GetListMemberOfFanpageDto input)
        {
            try
            {
                var getListResult = await _httpSocialMediaService.GetListMemberIdOfFanpage(input);
                return DataResult.ResultSuccess(getListResult.Result.Items, getListResult.Message, getListResult.Result.TotalCount);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> CreateFanpageAsync(CreateFanpageByAdminDto input)
        {
            try
            {
                var createResult = await _httpSocialMediaService.CreateFanpage(input);
                return DataResult.ResultSuccess(createResult.Result, createResult.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
        
        public async Task<object> UpdateFanpageAsync(UpdateFanpageByAdminDto input)
        {
            try
            {
                var updateResult = await _httpSocialMediaService.UpdateFanpage(input);
                return DataResult.ResultSuccess(updateResult.Result, updateResult.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
        
        public async Task<object> UpdateMemberOfFanpageAsync(UpdateMemberOfFanpageDto input)
        {
            try
            {
                var updateResult = await _httpSocialMediaService.UpdateMemberOfFanpage(input);
                return DataResult.ResultSuccess(updateResult.Result, updateResult.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
        
        public async Task<object> DeleteFanpageAsync(DeleteFanpageDto input)
        {
            try
            {
                var deleteResult = await _httpSocialMediaService.DeleteFanpage(input);
                return DataResult.ResultSuccess(deleteResult.Result, deleteResult.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
        
        public async Task<object> DeleteMemberOfFanpageAsync(DeleteMemberOfFanpageDto input)
        {
            try
            {
                var deleteResult = await _httpSocialMediaService.DeleteMemberOfFanpage(input);
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