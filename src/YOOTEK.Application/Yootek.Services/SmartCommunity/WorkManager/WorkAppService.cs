using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Yootek.App.ServiceHttpClient.Dto;
using Yootek.App.ServiceHttpClient.Dto.Yootek.SmartCommunity.WorkDtos;
using Yootek.App.ServiceHttpClient.Yootek.SmartCommunity;
using Yootek.Authorization.Users;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Yootek.Notifications;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Yootek.Common.Enum.UserFeedbackEnum;
using Yootek.Core.Dto;
using Yootek.Services.ExportData;

namespace Yootek.Services
{
    public interface IWorkAppService
    {
        Task<DataResult> GetWorkStatisticAsync([FromQuery] GetWorkStatisticDto input);
        Task<DataResult> GetWorkStatisticGeneralAsync([FromQuery] GetWorkStatisticGeneralDto input);
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
        Task<DataResult> ExportExcel([FromBody] GetExcelWorkDto input);
    }
    public class WorkAppService : YootekAppServiceBase, IWorkAppService
    {
        private readonly IHttpWorkAssignmentService _httpWorkAssignmentService;
        private readonly IRepository<User, long> _userRepository;
        private readonly IAdminCitizenReflectAppService _adminCitizenReflectAppService;
        private readonly IDigitalServiceOrderAppService _digitalServiceOrderAppService;
        private readonly IWorkExcelExporter _repositoryExcelExporter;
        private readonly UserManager _userManager;
        private readonly IAppNotifier _appNotifier;
        public WorkAppService(
            IHttpWorkAssignmentService httpWorkAssignmentService,
            IRepository<User, long> userRepository,
             UserManager userManager,
            IAdminCitizenReflectAppService adminCitizenReflectAppService, IDigitalServiceOrderAppService digitalServiceOrderAppService, IAppNotifier appNotifier, IWorkExcelExporter repositoryExcelExporter)
        {
            _httpWorkAssignmentService = httpWorkAssignmentService;
            _userRepository = userRepository;
            _adminCitizenReflectAppService = adminCitizenReflectAppService;
            _digitalServiceOrderAppService = digitalServiceOrderAppService;
            _userManager = userManager;
            _appNotifier = appNotifier;
            _repositoryExcelExporter = repositoryExcelExporter;
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
                throw;
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
        public async Task<DataResult> GetWorkStatisticGeneralAsync([FromQuery] GetWorkStatisticGeneralDto input)
        {
            try
            {
                MicroserviceResultDto<Dictionary<string, int>> result = await _httpWorkAssignmentService.GetWorkStatisticGeneral(input);
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
        public async Task<DataResult> GetListWorkPlan([FromQuery] GetAllWorksPlanQuery input)
        {
            try
            {
                MicroserviceResultDto<PagedResultDto<GetAllWorksPlanDto>> listResult = await _httpWorkAssignmentService.GetListWorkPlan(input);
                List<GetAllWorksPlanDto> result = listResult.Result.Items.ToList();
                foreach (GetAllWorksPlanDto item in result)
                {
                    foreach (var work in item.WorkPlanValues)
                    {
                        work.FullName = GetFullNameAndUserNameOfUser(work.CreatorUserId);
                    }
                };
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
        private string GetFullNameAndUserNameOfUser(long userId)
        {
            User user = _userManager.Users.FirstOrDefault(x => x.Id == userId);
            return (user?.FullName ?? string.Empty) + " (" + (user?.UserName ?? string.Empty) + ")";
        }
                
        public async Task<DataResult> ExportExcel([FromBody] GetExcelWorkDto input)
        {
            try
            {
                var result = await _httpWorkAssignmentService.GetListWorkExcel(input);
                var data = result.Result;
                var query = _userRepository.GetAll().AsQueryable();
                for (int i=0;i<data.Count();i++)
                {
                    
                    WorkDetailUserDto creatorUser = query.Where(x => x.Id == data[i].CreatorUserId).Select(
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
                    List<WorkDetailUserDto> recipientUser = query.Where(x => data[i].RecipientIds.Contains(x.Id)).Select(
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
                    List<WorkDetailUserDto> supervisorUser = query.Where(x => data[i].SupervisorIds.Contains(x.Id)).Select(
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

                    data[i].CreatorUser = creatorUser;
                    data[i].RecipientUsers = recipientUser;
                    data[i].SupervisorUsers = supervisorUser;
                }                
                FileDto file = _repositoryExcelExporter.ExportToFile(data);
                return DataResult.ResultSuccess(file, "Export excel success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

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
                Frequency = input.Result.Frequency,
                FrequencyOption = input.Result.FrequencyOption,
                RemindWork = input.Result.RemindWork
            };
        }
        #endregion
    }
}
