﻿using Abp;
using Abp.Application.Services.Dto;
using IMAX.App.ServiceHttpClient.Dto;
using IMAX.App.ServiceHttpClient.Dto.IMAX.SmartCommunity.WorkDtos;
using IMAX.App.ServiceHttpClient.IMAX.SmartCommunity;
using IMAX.Authorization.Users;
using IMAX.Common.DataResult;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IMAX.Services
{
    public interface IWorkLogTimeAppService
    {
        Task<DataResult> GetListWorkLogTimeAsync([FromQuery] GetAllWorkLogTimeDto input);
        Task<DataResult> GetWorkLogTimeByIdAsync(GetWorkLogTimeByIdDto input);
        Task<DataResult> CreateWorkLogTimeAsync([FromBody] CreateWorkLogTimeDto input);
        Task<DataResult> CreateManyWorkLogTimeAsync([FromBody] CreateManyWorkLogTimeDto input);
        Task<DataResult> UpdateWorkLogTimeAsync([FromBody] UpdateWorkLogTimeDto input);
        Task<DataResult> UpdateStatusWorkLogTimeAsync([FromBody] UpdateStatusWorkLogTimeDto input);
        Task<DataResult> UpdateManyWorkLogTimeAsync([FromBody] UpdateManyWorkLogTimeDto input);
        Task<DataResult> DeleteWorkLogTimeAsync([FromQuery] DeleteWorkLogTimeDto input);
    }

    public class WorkLogTimeAppService : IMAXAppServiceBase, IWorkLogTimeAppService
    {
        private readonly IHttpWorkAssignmentService _httpWorkAssignmentService;
        private readonly UserManager _userManager;
        public WorkLogTimeAppService(
            IHttpWorkAssignmentService httpWorkAssignmentService,
            UserManager userManager
            )
        {
            _httpWorkAssignmentService = httpWorkAssignmentService;
            _userManager = userManager;
        }

        public async Task<DataResult> GetListWorkLogTimeAsync(GetAllWorkLogTimeDto input)
        {
            try
            {
                MicroserviceResultDto<PagedResultDto<WorkLogTimeDto>> listResult =
                    await _httpWorkAssignmentService.GetListWorkLogTime(input);
                List<WorkLogTimeDto> result = listResult.Result.Items.ToList();
                foreach (WorkLogTimeDto item in result)
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
        public async Task<DataResult> GetWorkLogTimeByIdAsync(GetWorkLogTimeByIdDto input)
        {
            try
            {
                MicroserviceResultDto<WorkLogTimeDto> result =
                    await _httpWorkAssignmentService.GetWorkLogTimeById(input);
                result.Result.FullName = GetFullNameOfUser(result.Result.CreatorUserId);
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
        public async Task<DataResult> CreateWorkLogTimeAsync(CreateWorkLogTimeDto input)
        {
            try
            {
                MicroserviceResultDto<bool> result = await _httpWorkAssignmentService.CreateWorkLogTime(input);
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
        public async Task<DataResult> CreateManyWorkLogTimeAsync(CreateManyWorkLogTimeDto input)
        {
            try
            {
                MicroserviceResultDto<bool> result = await _httpWorkAssignmentService.CreateManyWorkLogTime(input);
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
        public async Task<DataResult> UpdateWorkLogTimeAsync(UpdateWorkLogTimeDto input)
        {
            try
            {
                MicroserviceResultDto<bool> result = await _httpWorkAssignmentService.UpdateWorkLogTime(input);
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
        public async Task<DataResult> UpdateStatusWorkLogTimeAsync(UpdateStatusWorkLogTimeDto input)
        {
            try
            {
                MicroserviceResultDto<bool> result = await _httpWorkAssignmentService.UpdateStatusWorkLogTime(input);
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
        public async Task<DataResult> UpdateManyWorkLogTimeAsync(UpdateManyWorkLogTimeDto input)
        {
            try
            {
                MicroserviceResultDto<bool> result = await _httpWorkAssignmentService.UpdateManyWorkLogTime(input);
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
        public async Task<DataResult> DeleteWorkLogTimeAsync(DeleteWorkLogTimeDto input)
        {
            try
            {
                MicroserviceResultDto<bool> result = await _httpWorkAssignmentService.DeleteWorkLogTime(input);
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
