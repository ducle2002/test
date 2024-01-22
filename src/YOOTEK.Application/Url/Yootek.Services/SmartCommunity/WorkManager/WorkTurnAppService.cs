using Abp;
using Abp.Application.Services.Dto;
using Yootek.App.ServiceHttpClient.Dto;
using Yootek.App.ServiceHttpClient.Dto.Yootek.SmartCommunity.WorkDtos;
using Yootek.App.ServiceHttpClient.Yootek.SmartCommunity;
using Yootek.Common.DataResult;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Yootek.Services
{
    public interface IWorkTurnAppService
    {
        Task<DataResult> GetListWorkTurnAsync([FromQuery] GetAllWorkTurnsDto input);
        Task<DataResult> GetListWorkTurnNotPagingAsync([FromQuery] GetAllWorkTurnsDto input);
        Task<DataResult> GetWorkTurnByIdAsync([FromQuery] GetWorkTurnByIdDto input);
        Task<DataResult> CreateWorkTurnAsync([FromBody] CreateWorkTurnDto input);
        Task<DataResult> UpdateWorkTurnAsync([FromBody] UpdateWorkTurnDto input);
        Task<DataResult> DeleteWorkTurnAsync([FromQuery] DeleteWorkTurnDto input);
        Task<DataResult> DeleteManyWorkTurnAsync(DeleteManyWorkTurnDto input);
    }

    public class WorkTurnAppService : YootekAppServiceBase, IWorkTurnAppService
    {
        private readonly IHttpWorkAssignmentService _httpWorkAssignmentService;

        public WorkTurnAppService(IHttpWorkAssignmentService httpWorkAssignmentService)
        {
            _httpWorkAssignmentService = httpWorkAssignmentService;
        }


        public async Task<DataResult> GetListWorkTurnAsync(GetAllWorkTurnsDto input)
        {
            try
            {
                MicroserviceResultDto<PagedResultDto<WorkTurnDto>> listResult = await _httpWorkAssignmentService.GetListWorkTurn(input);
                return DataResult.ResultSuccess(listResult.Result.Items, listResult.Message, listResult.Result.TotalCount);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
        public async Task<DataResult> GetListWorkTurnNotPagingAsync([FromQuery] GetAllWorkTurnsDto input)
        {
            try
            {
                MicroserviceResultDto<PagedResultDto<WorkTurnDto>> listResult = await _httpWorkAssignmentService.GetListWorkTurnNotPaging(input);
                return DataResult.ResultSuccess(listResult.Result.Items, listResult.Message, listResult.Result.TotalCount);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
        public async Task<DataResult> GetWorkTurnByIdAsync(GetWorkTurnByIdDto input)
        {
            try
            {
                MicroserviceResultDto<WorkTurnDto> result = await _httpWorkAssignmentService.GetWorkTurnById(input);
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<DataResult> CreateWorkTurnAsync(CreateWorkTurnDto input)
        {
            try
            {
                MicroserviceResultDto<bool> result = await _httpWorkAssignmentService.CreateWorkTurn(input);
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<DataResult> UpdateWorkTurnAsync(UpdateWorkTurnDto input)
        {
            try
            {
                MicroserviceResultDto<bool> result = await _httpWorkAssignmentService.UpdateWorkTurn(input);
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<DataResult> DeleteWorkTurnAsync(DeleteWorkTurnDto input)
        {
            try
            {
                MicroserviceResultDto<bool> result = await _httpWorkAssignmentService.DeleteWorkTurn(input);
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<DataResult> DeleteManyWorkTurnAsync(DeleteManyWorkTurnDto input)
        {
            try
            {
                MicroserviceResultDto<bool> result = await _httpWorkAssignmentService.DeleteManyWorkTurn(input);
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
