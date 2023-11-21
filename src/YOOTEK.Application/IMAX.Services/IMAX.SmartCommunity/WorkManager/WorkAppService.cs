using Abp;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
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
using static IMAX.Common.Enum.UserFeedbackEnum;

namespace IMAX.Services
{
    public interface IWorkAppService
    {
        Task<DataResult> GetWorkStatisticAsync([FromQuery] GetWorkStatisticDto input);
        // Task<DataResult> GetListWorkByAdminAsync([FromQuery] GetListWorkInput input);
        // Task<DataResult> GetWorkByIdByAdminAsync([FromQuery] long id);
        Task<DataResult> GetListWorkAsync([FromQuery] GetListWorkInput input);
        Task<DataResult> GetWorkByIdAsync([FromQuery] long id);
        Task<DataResult> CreateWorkAsync([FromBody] CreateWorkDto input);
        Task<DataResult> AssignWorkAsync([FromBody] AssignWorkDto input);
        Task<DataResult> UpdateWorkAsync([FromBody] UpdateWorkDto input);
        Task<DataResult> UpdateStateWorkAsync([FromBody] UpdateStateWorkDto input);
        Task<DataResult> DeleteWorkAsync([FromQuery] DeleteWorkDto input);
        Task<DataResult> DeleteManyWorkAsync([FromQuery] List<long> ids);
    }
    public class WorkAppService : IMAXAppServiceBase, IWorkAppService
    {
        private readonly IHttpWorkAssignmentService _httpWorkAssignmentService;
        private readonly IRepository<User, long> _userRepository;
        private readonly IAdminCitizenReflectAppService _adminCitizenReflectAppService;
        private readonly IDigitalServiceOrderAppService _digitalServiceOrderAppService;
        public WorkAppService(
            IHttpWorkAssignmentService httpWorkAssignmentService,
            IRepository<User, long> userRepository,
            IAdminCitizenReflectAppService adminCitizenReflectAppService, IDigitalServiceOrderAppService digitalServiceOrderAppService)
        {
            _httpWorkAssignmentService = httpWorkAssignmentService;
            _userRepository = userRepository;
            _adminCitizenReflectAppService = adminCitizenReflectAppService;
            _digitalServiceOrderAppService = digitalServiceOrderAppService;
        }
        /*public async Task<DataResult> GetListWorkByAdminAsync([FromQuery] GetListWorkInput input)
        {
            try
            {
                MicroserviceResultDto<PagedResultDto<WorkDto>> listResult = await _httpWorkAssignmentService.AdminGetListWork(new()
                {
                    FormId = (int?)input.FormId,
                    Status = (int?)input.Status,
                    MaxResultCount = input.MaxResultCount,
                    SkipCount = input.SkipCount
                });
                return DataResult.ResultSuccess(listResult.Result.Items, listResult.Message, listResult.Result.TotalCount);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }*/
        /*public async Task<DataResult> GetWorkByIdByAdminAsync([FromQuery] long id)
        {
            try
            {
                MicroserviceResultDto<WorkDetailDto> result = await _httpWorkAssignmentService.AdminGetWorkById(id);
                var workDetail = await GetWorkUserInfor(result);
                return DataResult.ResultSuccess(workDetail, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }*/

        public async Task<DataResult> GetWorkStatisticAsync([FromQuery] GetWorkStatisticDto input)
        {
            try
            {
                MicroserviceResultDto<WorkStatisticDto> result = await _httpWorkAssignmentService.GetWorkStatistic(new()
                {
                    StartDate = input.StartDate,
                    EndDate = input.EndDate
                });
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<DataResult> GetListWorkAsync([FromQuery] GetListWorkInput input)
        {
            try
            {
                MicroserviceResultDto<PagedResultDto<WorkDto>> listResult = await _httpWorkAssignmentService.GetListWork(new GetListWorkDto()
                {
                    FormId = (int?)input.FormId,
                    Status = (int?)input.Status,
                    WorkTypeId = input.WorkTypeId,
                    Keyword = input.Keyword,
                    MaxResultCount = input.MaxResultCount,
                    SkipCount = input.SkipCount
                });
                return DataResult.ResultSuccess(listResult.Result.Items, listResult.Message, listResult.Result.TotalCount);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<DataResult> GetWorkByIdAsync([FromQuery] long id)
        {
            try
            {
                MicroserviceResultDto<WorkDetailDto> result = await _httpWorkAssignmentService.GetWorkById(id);
                var workDetail = await GetWorkUserInfor(result);
                return DataResult.ResultSuccess(workDetail, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
        public async Task<DataResult> CreateWorkAsync([FromBody] CreateWorkDto input)
        {
            try
            {
                MicroserviceResultDto<long?> result = await _httpWorkAssignmentService.CreateWork(input);
                if (result?.Result != null && result?.Success == true && input.Items?.Count > 0)
                {
                    foreach (CreateWorkAssociationDto oCreateWorkAssociationDto in input.Items)
                    {
                        switch (oCreateWorkAssociationDto.RelationshipType)
                        {
                            // Khi tạo công việc mới thành công sẽ cập nhật trạng thái dịch vụ nội khu sang đang xử lý
                            case WorkAssociationType.DIGITAL_SERVICES:
                                await _digitalServiceOrderAppService.UpdateState(new UpdateStateDigitalServiceOrderDto()
                                {
                                    Id = oCreateWorkAssociationDto.RelatedId,
                                    TypeAction = TypeActionUpdateStateServiceOrder.START_DOING,
                                });
                                break;
                            default:
                                break;
                        }
                    }
                }
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
        public async Task<DataResult> AssignWorkAsync([FromBody] AssignWorkDto input)
        {
            try
            {
                MicroserviceResultDto<bool> result = await _httpWorkAssignmentService.AssignWork(input);
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
        public async Task<DataResult> UpdateWorkAsync([FromBody] UpdateWorkDto input)
        {
            try
            {
                MicroserviceResultDto<bool> result = await _httpWorkAssignmentService.UpdateWork(input);
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
        public async Task<DataResult> UpdateStateWorkAsync([FromBody] UpdateStateWorkDto input)
        {
            try
            {
                MicroserviceResultDto<List<UpdateStateRelateDto>> result = await _httpWorkAssignmentService.UpdateStateWork(input);
                if (result?.Success == true && result?.Result != null)
                {
                    List<UpdateStateRelateDto> updateStateRelateDtos = result.Result;
                    foreach (UpdateStateRelateDto updateStateRelateDto in updateStateRelateDtos)
                    {
                        switch (updateStateRelateDto.RelationshipType)
                        {
                            case WorkAssociationType.REFLECT:
                                await _adminCitizenReflectAppService.UpdateStateCitizenReflect(new UpdateStateReflectInput()
                                {
                                    Id = updateStateRelateDto.Id,
                                    State = STATE_FEEDBACK.ADMIN_CONFIRMED,
                                });
                                break;
                            case WorkAssociationType.DIGITAL_SERVICES:
                                if ((int)input.TypeAction == (int)TypeActionUpdateStateWork.COMPLETE)
                                {
                                    //Khi công việc hoàn thành sẽ cập nhật trạng thái dịch vụ nội khu thành đã hoàn thành
                                    await _digitalServiceOrderAppService.UpdateState(new UpdateStateDigitalServiceOrderDto()
                                    {
                                        Id = updateStateRelateDto.Id,
                                        TypeAction = TypeActionUpdateStateServiceOrder.COMPLETE,
                                    });
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
                return DataResult.ResultSuccess(true, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
        public async Task<DataResult> DeleteWorkAsync([FromQuery] DeleteWorkDto input)
        {
            try
            {
                MicroserviceResultDto<bool> result = await _httpWorkAssignmentService.DeleteWork(input);
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
        public async Task<DataResult> DeleteManyWorkAsync([FromBody] List<long> ids)
        {
            try
            {
                MicroserviceResultDto<bool> result = await _httpWorkAssignmentService.DeleteManyWork(new DeleteManyWorkDto() { Ids = ids });
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        #region method helpers
        private async Task<object> GetWorkUserInfor(MicroserviceResultDto<WorkDetailDto> input)
        {
            var query = _userRepository.GetAll().AsQueryable();
            WorkDetailUserDto creatorUser = query.Where(x => x.Id == input.Result.WorkCreatorId).Select(
                x => new WorkDetailUserDto()
                {
                    Id = x.Id,
                    FullName = x.FullName,
                    AvatarUrl = x.ImageUrl,
                    TenantId = x.TenantId,
                    UserName = x.UserName,
                    EmailAddress = x.EmailAddress
                }
            ).FirstOrDefault();

            List<WorkDetailUserDto> recipientUser = query.Where(x => input.Result.RecipientIds.Contains(x.Id)).Select(
                x => new WorkDetailUserDto()
                {
                    Id = x.Id,
                    FullName = x.FullName,
                    AvatarUrl = x.ImageUrl,
                    TenantId = x.TenantId,
                    UserName = x.UserName,
                    EmailAddress = x.EmailAddress
                }
            ).ToList();
            List<WorkDetailUserDto> supervisorUser = query.Where(x => input.Result.SupervisorIds.Contains(x.Id)).Select(
                x => new WorkDetailUserDto()
                {
                    Id = x.Id,
                    FullName = x.FullName,
                    AvatarUrl = x.ImageUrl,
                    TenantId = x.TenantId,
                    UserName = x.UserName,
                    EmailAddress = x.EmailAddress
                }
            ).ToList();

            return new WorkDetailWithUserNameDto()
            {
                Id = input.Result.Id,
                TenantId = input.Result.TenantId,
                Title = input.Result.Title,
                Content = input.Result.Content,
                ImageUrls = input.Result.ImageUrls,
                Note = input.Result.Note,
                WorkTypeId = input.Result.WorkTypeId,
                Status = input.Result.Status,
                DateStart = input.Result.DateStart,
                DateExpected = input.Result.DateExpected,
                DateFinish = input.Result.DateFinish,
                WorkCreatorId = input.Result.WorkCreatorId,
                CreatorUser = creatorUser,
                RecipientUsers = recipientUser,
                SupervisorUsers = supervisorUser,
                WorkLogTimes = input.Result.WorkLogTimes,
                WorkHistories = input.Result.WorkHistories,
                CreationTime = input.Result.CreationTime,
                ListWorkDetail = input.Result.ListWorkDetail,
                QrCode = input.Result.QrCode,
            };
        }
        #endregion
    }
}
