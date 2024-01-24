using Abp;
using Abp.Application.Services.Dto;
using Yootek.App.ServiceHttpClient;
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
    public interface IWorkDetailAppService
    {
        Task<DataResult> GetListWorkDetailAsync([FromQuery] GetAllWorkDetailDto input);
        Task<DataResult> GetListWorkDetailNotPagingAsync(GetAllWorkDetailNotPagingDto input);
        Task<DataResult> CreateWorkDetailAsync([FromBody] CreateWorkDetailDto input);
        Task<DataResult> UpdateWorkDetailAsync([FromBody] UpdateWorkDetailDto input);
        Task<DataResult> DeleteWorkDetailAsync([FromQuery] DeleteWorkDetailDto input);
    }

    public class WorkDetailAppService : YootekAppServiceBase, IWorkDetailAppService
    {
        private readonly IHttpWorkAssignmentService _httpWorkAssignmentService;
        private readonly IHttpQRCodeService _httpQRCodeService;

        public WorkDetailAppService(
            IHttpWorkAssignmentService httpWorkAssignmentService,
            IHttpQRCodeService httpQRCodeService
            )
        {
            _httpWorkAssignmentService = httpWorkAssignmentService;
            _httpQRCodeService = httpQRCodeService;
        }


        public async Task<DataResult> GetListWorkDetailAsync(GetAllWorkDetailDto input)
        {
            try
            {
                MicroserviceResultDto<PagedResultDto<DetailWorkDto>> listResult =
                    await _httpWorkAssignmentService.GetListWorkDetail(input);
                return DataResult.ResultSuccess(listResult.Result.Items, listResult.Message, listResult.Result.TotalCount);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
        public async Task<DataResult> GetListWorkDetailNotPagingAsync(GetAllWorkDetailNotPagingDto input)
        {
            try
            {
                MicroserviceResultDto<List<DetailWorkDto>> listResult =
                    await _httpWorkAssignmentService.GetListWorkDetailNotPaging(input);
                return DataResult.ResultSuccess(listResult.Result, listResult.Message, listResult.Result.Count);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
        public async Task<DataResult> CreateWorkDetailAsync(CreateWorkDetailDto input)
        {
            try
            {
                MicroserviceResultDto<bool> result = await _httpWorkAssignmentService.CreateWorkDetail(input);
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<DataResult> UpdateWorkDetailAsync(UpdateWorkDetailDto input)
        {
            try
            {
                MicroserviceResultDto<bool> result = await _httpWorkAssignmentService.UpdateWorkDetail(input);
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<DataResult> DeleteWorkDetailAsync(DeleteWorkDetailDto input)
        {
            try
            {
                MicroserviceResultDto<bool> result = await _httpWorkAssignmentService.DeleteWorkDetail(input);
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
