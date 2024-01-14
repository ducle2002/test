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
    public interface IWorkCommentAppService
    {
        Task<DataResult> GetListWorkCommentAsync([FromQuery] GetAllWorkCommentDto input);
        Task<DataResult> GetListWorkCommentNotPagingAsync(GetAllWorkCommentNotPagingDto input);
        Task<DataResult> GetWorkCommentByIdAsync([FromQuery] GetWorkCommentDto input);
        Task<DataResult> CreateWorkCommentAsync([FromBody] CreateWorkCommentDto input);
        Task<DataResult> UpdateWorkCommentAsync([FromBody] UpdateWorkCommentDto input);
        Task<DataResult> DeleteWorkCommentAsync([FromQuery] DeleteWorkCommentDto input);
    }

    public class WorkCommentAppService : YootekAppServiceBase, IWorkCommentAppService
    {
        private readonly IHttpWorkAssignmentService _httpWorkAssignmentService;
        private readonly UserManager _userManager;

        public WorkCommentAppService(IHttpWorkAssignmentService httpWorkAssignmentService, UserManager userManager)
        {
            _httpWorkAssignmentService = httpWorkAssignmentService;
            _userManager = userManager;
        }


        public async Task<DataResult> GetListWorkCommentAsync(GetAllWorkCommentDto input)
        {
            try
            {
                MicroserviceResultDto<PagedResultDto<WorkCommentDto>> listResult = await _httpWorkAssignmentService.GetListWorkComment(input);
                List<WorkCommentDto> result = listResult.Result.Items.ToList();
                foreach (WorkCommentDto item in result)
                {
                    item.FullName = GetFullNameOfUser(item.CreatorUserId);
                };
                return DataResult.ResultSuccess(listResult.Result.Items, listResult.Message, listResult.Result.TotalCount);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
        public async Task<DataResult> GetListWorkCommentNotPagingAsync(GetAllWorkCommentNotPagingDto input)
        {
            try
            {
                MicroserviceResultDto<List<WorkCommentDto>> listResult = await _httpWorkAssignmentService.GetListWorkCommentNotPaging(input);
                List<WorkCommentDto> result = listResult.Result.ToList();
                foreach (WorkCommentDto item in result)
                {
                    item.FullName = GetFullNameOfUser(item.CreatorUserId);
                };
                return DataResult.ResultSuccess(listResult.Result, listResult.Message, listResult.Result.Count);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
        public async Task<DataResult> GetWorkCommentByIdAsync(GetWorkCommentDto input)
        {
            try
            {
                MicroserviceResultDto<WorkCommentDetailDto> result = await _httpWorkAssignmentService.GetWorkComment(input);
                result.Result.FullName = GetFullNameOfUser(result.Result.CreatorUserId);
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<DataResult> CreateWorkCommentAsync(CreateWorkCommentDto input)
        {
            try
            {
                MicroserviceResultDto<bool> result = await _httpWorkAssignmentService.CreateWorkComment(input);
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<DataResult> UpdateWorkCommentAsync(UpdateWorkCommentDto input)
        {
            try
            {
                MicroserviceResultDto<bool> result = await _httpWorkAssignmentService.UpdateWorkComment(input);
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<DataResult> DeleteWorkCommentAsync(DeleteWorkCommentDto input)
        {
            try
            {
                MicroserviceResultDto<bool> result = await _httpWorkAssignmentService.DeleteWorkComment(input);
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
