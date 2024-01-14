using System;
using System.Threading.Tasks;
using Abp;
using Yootek.App.ServiceHttpClient.Dto.Social.Forum;
using Yootek.App.ServiceHttpClient.Social;
using Yootek.Common.DataResult;

namespace Yootek.Yootek.Services.Yootek.SmartSocial.YooSocialMedia
{
    public interface ISocialMediaGroup
    {
        Task<object> GetListGroupByUserAsync(GetListGroupByUserDto input);
        Task<object> GetListMemberOfGroupAsync(GetListMemberOfGroupDto input);
        Task<object> GetById (GetGroupByIdDto input);
        Task<object> GetListUser(GetSocialMediaUserDto input);
        Task<object> CreateGroupAsync(CreateGroupByAdminDto input);
        Task<object> UpdateGroupAsync(UpdateGroupByAdminDto input);
        Task<object> UpdateGroupMemberAsync(UpdateMemberOfGroupDto input);
        Task<object> DeleteGroupAsync(DeleteGroupByAdminDto input);
        Task<object> DeleteGroupMemberAsync(DeleteGroupMemberDto input);
    }
    public class SocialMediaGroupAppService : YootekAppServiceBase, ISocialMediaGroup
    {
        private readonly IHttpSocialMediaService _httpSocialMediaService;
        
        public SocialMediaGroupAppService(IHttpSocialMediaService httpSocialMediaService)
        {
            _httpSocialMediaService = httpSocialMediaService;
        } 
        
        public async Task<object> GetListGroupByUserAsync(GetListGroupByUserDto input)
        {
            try
            {
                var getListResult = await _httpSocialMediaService.GetListGroupByUser(input);
                return DataResult.ResultSuccess(getListResult.Result.Items, getListResult.Message, getListResult.Result.TotalCount);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> GetListMemberOfGroupAsync(GetListMemberOfGroupDto input)
        {
            try
            {
                var getListResult = await _httpSocialMediaService.GetListMemberOfGroup(input);
                return DataResult.ResultSuccess(getListResult.Result.Items, getListResult.Message, getListResult.Result.TotalCount);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
        
        public async Task<object> GetById(GetGroupByIdDto input)
        {
            try
            {
                var getResult = await _httpSocialMediaService.GetById(input);
                return DataResult.ResultSuccess(getResult.Result, getResult.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
        
        public async Task<object> GetListUser(GetSocialMediaUserDto input)
        {
            try
            {
                var getListResult = await _httpSocialMediaService.GetListUser(input);
                return DataResult.ResultSuccess(getListResult.Result.Items, getListResult.Message, getListResult.Result.TotalCount);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> CreateGroupAsync(CreateGroupByAdminDto input)
        {
            try
            {
                var createResult = await _httpSocialMediaService.CreateGroup(input);
                return DataResult.ResultSuccess(createResult.Result, createResult.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> UpdateGroupAsync(UpdateGroupByAdminDto input)
        {
            try
            {
                var updateResult = await _httpSocialMediaService.UpdateGroup(input);
                return DataResult.ResultSuccess(updateResult.Result, updateResult.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
        
        public async Task<object> UpdateGroupMemberAsync(UpdateMemberOfGroupDto input)
        {
            try
            {
                var updateResult = await _httpSocialMediaService.UpdateMemberOfGroup(input);
                return DataResult.ResultSuccess(updateResult.Result, updateResult.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> DeleteGroupAsync(DeleteGroupByAdminDto input)
        {
            try
            {
                var deleteResult = await _httpSocialMediaService.DeleteGroup(input);
                return DataResult.ResultSuccess(deleteResult.Result, deleteResult.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
        
        public async Task<object> DeleteGroupMemberAsync(DeleteGroupMemberDto input)
        {
            try
            {
                var deleteResult = await _httpSocialMediaService.DeleteMemberOfGroup(input);
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