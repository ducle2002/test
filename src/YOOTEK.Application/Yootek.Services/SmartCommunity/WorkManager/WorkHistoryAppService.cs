using Abp;
using Abp.Application.Services.Dto;
using Yootek.App.ServiceHttpClient.Dto;
using Yootek.App.ServiceHttpClient.Dto.Yootek.SmartCommunity.WorkDtos;
using Yootek.App.ServiceHttpClient.Yootek.SmartCommunity;
using Yootek.Authorization.Users;
using Yootek.Common.DataResult;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Yootek.Services
{
    public interface IWorkHistoryAppService
    {
        Task<DataResult> GetListWorkHistoryAsync([FromQuery] GetAllWorkHistoryDto input);
        Task<DataResult> GetWorkHistoryByIdAsync(GetWorkHistoryByIdDto input);
        Task<DataResult> CreateWorkHistoryAsync([FromBody] CreateWorkHistoryDto input);
        Task<DataResult> UpdateWorkHistoryAsync([FromBody] UpdateWorkHistoryDto input);
        Task<DataResult> DeleteWorkHistoryAsync([FromQuery] DeleteWorkHistoryDto input);
    }

    public class WorkHistoryAppService : YootekAppServiceBase, IWorkHistoryAppService
    {
        private readonly IHttpWorkAssignmentService _httpWorkAssignmentService;
        private readonly UserManager _userManager;

        public WorkHistoryAppService(
            IHttpWorkAssignmentService httpWorkAssignmentService,
            UserManager userManager)
        {
            _httpWorkAssignmentService = httpWorkAssignmentService;
            _userManager = userManager;
        }

        public async Task<DataResult> GetListWorkHistoryAsync(GetAllWorkHistoryDto input)
        {
            try
            {
                MicroserviceResultDto<PagedResultDto<WorkHistoryDto>> listResult =
                    await _httpWorkAssignmentService.GetListWorkHistory(input);
                List<WorkHistoryDto> result = listResult.Result.Items.ToList();
                foreach (WorkHistoryDto item in result)
                {
                    item.AssignerName = GetFullNameOfUser(item.AssignerId);
                    item.RecipientName = GetFullNameOfUser(item.RecipientId);
                    item.CreatorName = GetFullNameOfUser(item.CreatorUserId);
                };
                return DataResult.ResultSuccess(listResult.Result.Items, listResult.Message, listResult.Result.TotalCount);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
        public async Task<DataResult> GetWorkHistoryByIdAsync(GetWorkHistoryByIdDto input)
        {
            try
            {
                MicroserviceResultDto<WorkHistoryDto> result =
                    await _httpWorkAssignmentService.GetWorkHistoryById(input);
                result.Result.AssignerName = GetFullNameOfUser(result.Result.AssignerId);
                result.Result.RecipientName = GetFullNameOfUser(result.Result.RecipientId);
                result.Result.CreatorName = GetFullNameOfUser(result.Result.CreatorUserId);
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
        public async Task<DataResult> CreateWorkHistoryAsync(CreateWorkHistoryDto input)
        {
            try
            {
                MicroserviceResultDto<bool> result = await _httpWorkAssignmentService.CreateWorkHistory(input);
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<DataResult> UpdateWorkHistoryAsync(UpdateWorkHistoryDto input)
        {
            try
            {
                MicroserviceResultDto<bool> result = await _httpWorkAssignmentService.UpdateWorkHistory(input);
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<DataResult> DeleteWorkHistoryAsync(DeleteWorkHistoryDto input)
        {
            try
            {
                MicroserviceResultDto<bool> result = await _httpWorkAssignmentService.DeleteWorkHistory(input);
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        #region method helpers 
        private string GetFullNameOfUser(long userId)
        {
            User user = _userManager.Users.FirstOrDefault(x => x.Id == userId);
            return user?.Name ?? string.Empty;
        }
        #endregion
    }
}
