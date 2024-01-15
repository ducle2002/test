using Abp;
using Abp.Application.Services.Dto;
using Yootek.App.ServiceHttpClient.Dto;
using Yootek.App.ServiceHttpClient.Dto.Yootek.SmartCommunity.WorkDtos;
using Yootek.App.ServiceHttpClient.Yootek.SmartCommunity;
using Yootek.Common.DataResult;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Yootek.Services
{
    public interface IWorkTypeAppService
    {
        Task<DataResult> GetListWorkTypeAsync([FromQuery] GetAllWorkTypeDto input);
        Task<DataResult> GetListWorkTypeNotPagingAsync([FromQuery] GetAllWorkTypeNotPagingDto input);
        Task<DataResult> GetWorkTypeAsync([FromQuery] GetWorkTypeDto input);
        Task<DataResult> CreateWorkTypeAsync([FromBody] CreateWorkTypeDto input);
        Task<DataResult> UpdateWorkTypeAsync([FromBody] UpdateWorkTypeDto input);
        Task<DataResult> DeleteWorkTypeAsync([FromQuery] DeleteWorkTypeDto input);
    }

    public class WorkTypeAppService : YootekAppServiceBase, IWorkTypeAppService
    {
        private readonly IHttpWorkAssignmentService _httpWorkAssignmentService;

        public WorkTypeAppService(IHttpWorkAssignmentService httpWorkAssignmentService)
        {
            _httpWorkAssignmentService = httpWorkAssignmentService;
        }


        public async Task<DataResult> GetListWorkTypeAsync(GetAllWorkTypeDto input)
        {
            try
            {
                MicroserviceResultDto<PagedResultDto<WorkTypeDto>> listResult = await _httpWorkAssignmentService.GetListWorkType(input);
                return DataResult.ResultSuccess(listResult.Result.Items, listResult.Message, listResult.Result.TotalCount);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
        public async Task<DataResult> GetListWorkTypeNotPagingAsync([FromQuery] GetAllWorkTypeNotPagingDto input)
        {
            try
            {
                MicroserviceResultDto<List<WorkTypeDto>> listResult = await _httpWorkAssignmentService.GetListWorkTypeNotPaging(input);
                return DataResult.ResultSuccess(listResult.Result, listResult.Message, listResult.Result.Count);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
        public async Task<DataResult> GetWorkTypeAsync(GetWorkTypeDto input)
        {
            try
            {
                MicroserviceResultDto<WorkTypeDetailDto> result = await _httpWorkAssignmentService.GetWorkType(input);
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<DataResult> CreateWorkTypeAsync(CreateWorkTypeDto input)
        {
            try
            {
                MicroserviceResultDto<long> result = await _httpWorkAssignmentService.CreateWorkType(input);
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<DataResult> UpdateWorkTypeAsync(UpdateWorkTypeDto input)
        {
            try
            {
                MicroserviceResultDto<bool> result = await _httpWorkAssignmentService.UpdateWorkType(input);
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<DataResult> DeleteWorkTypeAsync(DeleteWorkTypeDto input)
        {
            try
            {
                MicroserviceResultDto<bool> result = await _httpWorkAssignmentService.DeleteWorkType(input);
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
    }
}