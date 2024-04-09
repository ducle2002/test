using Abp.Application.Services;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Yootek.App.ServiceHttpClient.Dto.Yootek.SmartCommunity.WorkDtos;
using Yootek.App.ServiceHttpClient.Dto;
using Yootek.Application;
using Yootek.Authorization.Users;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Microsoft.EntityFrameworkCore;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twilio.Rest.Api.V2010.Account;
using static Yootek.Common.Enum.CommonENum;
using Yootek.App.ServiceHttpClient.Yootek.SmartCommunity;
using Yootek.Authorization;
using Yootek.QueriesExtension;
using Yootek.Notifications;
using Abp;

namespace Yootek.Services
{
    public interface IDigitalServiceOrderAppService : IApplicationService
    {
        Task<DataResult> GetAllAsync(GetAllDigitalServiceOrderInputDto input);
        Task<DataResult> GetAllAdminAsync(GetAllDigitalServiceOrderInputDto input);
        Task<DataResult> GetById(long id);
        Task<DataResult> Create(DigitalServiceOrderCrearteDto input);
        Task<DataResult> Update(DigitalServiceOrderCrearteDto input);
        Task<DataResult> UpdateState(UpdateStateDigitalServiceOrderDto input);
        Task<DataResult> UpdateRate(UpdateRateDigitalServiceOrderDto input);
        Task<DataResult> UpdateFeedback(UpdateFeedbackDigitalServiceOrderDto input);
        Task<DataResult> Delete(long id);
    }

    [AbpAuthorize]
    public class DigitalServiceOrderAppService : YootekAppServiceBase, IDigitalServiceOrderAppService
    {
        private readonly IRepository<DigitalServiceOrder, long> _repository;
        private readonly IRepository<DigitalServices, long> _digitalServicesRepository;
        private readonly IRepository<Citizen, long> _citizenRepos;
        private readonly IHttpWorkAssignmentService _httpWorkAssignmentService;
        private readonly IAppNotifier _appNotifier;
        public DigitalServiceOrderAppService(
            IRepository<DigitalServiceOrder, long> repository,
            IRepository<DigitalServices, long> digitalServicesRepository, 
            IRepository<Citizen, long> citizenRepos, 
            IHttpWorkAssignmentService httpWorkAssignmentService, 
            IAppNotifier appNotifier
            )
        {
            _repository = repository;
            _digitalServicesRepository = digitalServicesRepository;
            _citizenRepos = citizenRepos;
            _httpWorkAssignmentService = httpWorkAssignmentService;
            _appNotifier = appNotifier;
        }

        public async Task<DataResult> GetAllAsync(GetAllDigitalServiceOrderInputDto input)
        {
            try
            {
                IQueryable<DigitalServiceOrderDto> query = (from o in _repository.GetAll()
                                                            select new DigitalServiceOrderDto
                                                            {
                                                                Id = o.Id,
                                                                Address = o.Address,
                                                                Note = o.Note,
                                                                Status = o.Status,
                                                                UrbanId = o.UrbanId,
                                                                TotalAmount = o.TotalAmount,
                                                                ResponseContent = o.ResponseContent,
                                                                RatingScore = o.RatingScore,
                                                                Comments = o.Comments,
                                                                ServiceId = o.ServiceId,
                                                                ServiceDetails = o.ServiceDetails,
                                                                ArrServiceDetails = !string.IsNullOrEmpty(o.ServiceDetails) ? JsonConvert.DeserializeObject<List<DigitalServiceDetailsGridDto>>(o.ServiceDetails) : new List<DigitalServiceDetailsGridDto>(),
                                                                TenantId = o.TenantId,
                                                                CreatorName = _citizenRepos.GetAll().Where(x => x.AccountId == o.CreatorUserId).Select(x => x.FullName).FirstOrDefault(),
                                                                ServiceText = _digitalServicesRepository.GetAll().Where(x => x.Id == o.ServiceId).Select(x => x.Title).FirstOrDefault(),
                                                                WorkTypeId = _digitalServicesRepository.GetAll().Where(x => x.Id == o.ServiceId).Select(x => x.WorkTypeId).FirstOrDefault(),
                                                                CreatorUserId = o.CreatorUserId,
                                                                OrderDate = o.OrderDate
                                                            })
                                                            .Where(x => x.CreatorUserId == AbpSession.UserId)
                                                            .WhereIf(!string.IsNullOrEmpty(input.Keyword), x => x.Address.ToLower().Contains(input.Keyword.ToLower()))
                                                            .WhereIf(input.Status > 0, x => x.Status == input.Status)
                                                            .WhereIf(input.StatusTab > 0, x => input.StatusTab == 1 ? x.Status == (int)TypeActionUpdateStateServiceOrder.CREATE : input.StatusTab == 2 ? (x.Status > (int)TypeActionUpdateStateServiceOrder.CREATE && x.Status < (int)TypeActionUpdateStateServiceOrder.COMPLETE) : x.Status > (int)TypeActionUpdateStateServiceOrder.FEEDBACK)
                                                            .WhereIf(input.ServiceId > 0, x => x.ServiceId == input.ServiceId);

                List<DigitalServiceOrderDto> result = await query.ApplySort(input.OrderBy, (SortBy)input.SortBy).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
                return DataResult.ResultSuccess(result, Common.Resource.QuanLyChung.GetAllSuccess, query.Count());
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> GetAllAdminAsync(GetAllDigitalServiceOrderInputDto input)
        {
            try
            {
                List<long> buIds = UserManager.GetAccessibleBuildingOrUrbanIds();
                IQueryable<DigitalServiceOrderDto> query = (from o in _repository.GetAll()
                                                            select new DigitalServiceOrderDto
                                                            {
                                                                Id = o.Id,
                                                                Address = o.Address,
                                                                Note = o.Note,
                                                                Status = o.Status,
                                                                UrbanId = o.UrbanId,
                                                                TotalAmount = o.TotalAmount,
                                                                ResponseContent = o.ResponseContent,
                                                                RatingScore = o.RatingScore,
                                                                Comments = o.Comments,
                                                                ServiceId = o.ServiceId,
                                                                ServiceDetails = o.ServiceDetails,
                                                                ArrServiceDetails = !string.IsNullOrEmpty(o.ServiceDetails) ? JsonConvert.DeserializeObject<List<DigitalServiceDetailsGridDto>>(o.ServiceDetails) : new List<DigitalServiceDetailsGridDto>(),
                                                                TenantId = o.TenantId,
                                                                CreatorName = _citizenRepos.GetAll().Where(x => x.AccountId == o.CreatorUserId).Select(x => x.FullName).FirstOrDefault(),
                                                                ServiceText = _digitalServicesRepository.GetAll().Where(x => x.Id == o.ServiceId).Select(x => x.Title).FirstOrDefault(),
                                                                WorkTypeId = _digitalServicesRepository.GetAll().Where(x => x.Id == o.ServiceId).Select(x => x.WorkTypeId).FirstOrDefault(),
                                                                OrderDate = o.OrderDate,
                                                            })
                                                            .WhereIf(!string.IsNullOrEmpty(input.Keyword), x => x.Address.ToLower().Contains(input.Keyword.ToLower()))
                                                            .WhereByBuildingOrUrbanIf(!IsGranted(IOCPermissionNames.Data_Admin), buIds)
                                                            .WhereIf(input.Status > 0, x => x.Status == input.Status)
                                                            .WhereIf(input.StatusTab > 0, x => input.StatusTab == 1 
                                                            ? x.Status == (int)TypeActionUpdateStateServiceOrder.CREATE : input.StatusTab == 2
                                                            ? (x.Status > (int)TypeActionUpdateStateServiceOrder.CREATE && x.Status < (int)TypeActionUpdateStateServiceOrder.COMPLETE) 
                                                            : x.Status > (int)TypeActionUpdateStateServiceOrder.FEEDBACK)
                                                            .WhereIf(input.ServiceId > 0, x => x.ServiceId == input.ServiceId);

                List<DigitalServiceOrderDto> result = await query.ApplySort(input.OrderBy, (SortBy)input.SortBy).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
                return DataResult.ResultSuccess(result, Common.Resource.QuanLyChung.GetAllSuccess, query.Count());
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> GetById(long id)
        {
            try
            {
                var item = await _repository.GetAsync(id);
                var data = item.MapTo<DigitalServiceOrderViewDto>();
                data.CreatorCitizen = await _citizenRepos.FirstOrDefaultAsync(x => x.AccountId == item.CreatorUserId);
                data.ArrServiceDetails = !string.IsNullOrEmpty(item.ServiceDetails) ? JsonConvert.DeserializeObject<List<DigitalServiceDetailsGridDto>>(item.ServiceDetails) : new List<DigitalServiceDetailsGridDto>();

                return DataResult.ResultSuccess(data, Common.Resource.QuanLyChung.Success);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<DataResult> GetWorkByOrderId(long id)
        {
            try
            {
                MicroserviceResultDto<List<WorkDto>> result = await _httpWorkAssignmentService.GetAllWorkByRelatedId(new GetListWorkByRelatedIdDto()
                {
                    RelatedId = id,
                    RelationshipType = WorkAssociationType.DIGITAL_SERVICES,
                });

                if (result != null)
                {
                    return DataResult.ResultSuccess(result.Result, Common.Resource.QuanLyChung.Success);
                }

                return DataResult.ResultSuccess( Common.Resource.QuanLyChung.Success);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> Create(DigitalServiceOrderCrearteDto dto)
        {
            try
            {
                var services = await _digitalServicesRepository.FirstOrDefaultAsync(dto.ServiceId);
                if (services == null) return DataResult.ResultCode(dto, "Digital service not found !", 404);

                DigitalServiceOrder item = ObjectMapper.Map<DigitalServiceOrderCrearteDto>(dto);
                if (dto.ArrServiceDetails != null)
                    item.ServiceDetails = JsonConvert.SerializeObject(dto.ArrServiceDetails);
              
                item.UrbanId = services.UrbanId;
                item.TenantId = AbpSession.TenantId;
                await _repository.InsertAndGetIdAsync(item);

                item.Code = GetUniqueKey(6) + item.Id;
                await CurrentUnitOfWork.SaveChangesAsync();
                var admins = await UserManager.GetUserOrganizationUnitByUrbanOrNull(item.UrbanId);
                if(admins != null)
                {
                    await FireNotificationCreateServiceOrder(item, services.Title, admins.ToArray());
                }
                return DataResult.ResultSuccess(Common.Resource.QuanLyChung.InsertSuccess);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> Update(DigitalServiceOrderCrearteDto dto)
        {
            try
            {
                DigitalServiceOrder item = await _repository.GetAsync(dto.Id);
                if (item != null)
                {
                    //dto.MapTo(item);
                    ObjectMapper.Map(dto, item);
                    item.TenantId = AbpSession.TenantId;
                    if (dto.ArrServiceDetails != null)
                        item.ServiceDetails = JsonConvert.SerializeObject(dto.ArrServiceDetails);
                    await _repository.UpdateAsync(item);
                    return DataResult.ResultSuccess(true, Common.Resource.QuanLyChung.UpdateSuccess);
                }
                else
                {
                    throw new UserFriendlyException(Common.Resource.QuanLyChung.UpdateFail);
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> UpdateState(UpdateStateDigitalServiceOrderDto dto)
        {
            try
            {
                DigitalServiceOrder item = await _repository.GetAsync(dto.Id);
                if (item != null)
                {
                    item.Status = (int)dto.TypeAction;
                    await _repository.UpdateAsync(item);
                    await FireNotificationStateServiceOrder(item);
                    return DataResult.ResultSuccess(true, "Update state success !");
                }
                else
                {
                    throw new UserFriendlyException(Common.Resource.QuanLyChung.UpdateFail);
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> Delete(long id)
        {
            try
            {
                await _repository.DeleteAsync(id);
                var data = DataResult.ResultSuccess(Common.Resource.QuanLyChung.DeleteSuccess);
                return data;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> UpdateRate(UpdateRateDigitalServiceOrderDto input)
        {
            try
            {
                DigitalServiceOrder item = await _repository.GetAsync(input.Id);
                if (item != null)
                {
                    item.RatingScore = input.RatingScore;
                    item.Comments = input.Comments;
                    await _repository.UpdateAsync(item);
                    return DataResult.ResultSuccess(true, Common.Resource.QuanLyChung.UpdateSuccess);
                }
                else
                {
                    throw new UserFriendlyException(Common.Resource.QuanLyChung.UpdateFail);
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> UpdateFeedback(UpdateFeedbackDigitalServiceOrderDto input)
        {
            try
            {
                DigitalServiceOrder item = await _repository.GetAsync(input.Id);
                if (item != null)
                {
                    item.TotalAmount = input.TotalAmount;
                    item.ResponseContent = input.ResponseContent;
                    item.Status = (int)TypeActionUpdateStateServiceOrder.FEEDBACK;
                    await _repository.UpdateAsync(item);
                    return DataResult.ResultSuccess(true, Common.Resource.QuanLyChung.UpdateSuccess);
                }
                else
                {
                    throw new UserFriendlyException(Common.Resource.QuanLyChung.UpdateFail);
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        private async Task FireNotificationStateServiceOrder(DigitalServiceOrder oDigitalServiceOrder)
        {
            try
            {
                string detailUrlApp = $"yoolife://app/digital-service-order/detail?id={oDigitalServiceOrder.Id}";
                switch (oDigitalServiceOrder.Status)
                {
                    case (int)TypeActionUpdateStateServiceOrder.RECEIVE:

                        var messageAccept = new UserMessageNotificationDataBase(
                            AppNotificationAction.DigitalServiceOrder,
                            AppNotificationIcon.DigitalServiceOrder,
                            TypeAction.Detail,
                            "Dịch vụ bạn đặt đã được tiếp nhận",
                            detailUrlApp,
                            ""

                        );
                        await _appNotifier.SendMessageNotificationInternalAsync(
                            "Yoolife dịch vụ nội khu !",
                            "Dịch vụ bạn đặt đã được tiếp nhận!",
                            detailUrlApp,
                            "",
                            new[] { new UserIdentifier(oDigitalServiceOrder.TenantId, oDigitalServiceOrder.CreatorUserId.Value) },
                            messageAccept,
                            AppType.USER);
                        break;
                    case (int)TypeActionUpdateStateServiceOrder.FEEDBACK:
                        var messageAcceptFeed = new UserMessageNotificationDataBase(
                             AppNotificationAction.DigitalServiceOrder,
                            AppNotificationIcon.DigitalServiceOrder,
                            TypeAction.Detail,
                            "Dịch vụ bạn đặt có phản hồi",
                            detailUrlApp,
                             ""

                        );
                        await _appNotifier.SendMessageNotificationInternalAsync(
                            "Yoolife dịch vụ nội khu !",
                            "Dịch vụ bạn đặt có phản hồi. Hãy xác nhận!",
                            detailUrlApp,
                            "",
                            new[] { new UserIdentifier(oDigitalServiceOrder.TenantId, oDigitalServiceOrder.CreatorUserId.Value) },
                            messageAcceptFeed,
                            AppType.USER);
                        break;
                    case (int)TypeActionUpdateStateServiceOrder.COMPLETE:
                        var messageAcceptCom = new UserMessageNotificationDataBase(
                             AppNotificationAction.DigitalServiceOrder,
                            AppNotificationIcon.DigitalServiceOrder,
                            TypeAction.Detail,
                            "Dịch vụ bạn đặt đã hoàn thành. Hãy thanh toán!",
                            detailUrlApp,
                            ""

                        );
                        await _appNotifier.SendMessageNotificationInternalAsync(
                            "Yoolife dịch vụ nội khu !",
                            "Dịch vụ bạn đặt đã hoàn thành. Hãy thanh toán!",
                            detailUrlApp,
                            "",
                            new[] { new UserIdentifier(oDigitalServiceOrder.TenantId, oDigitalServiceOrder.CreatorUserId.Value) },
                            messageAcceptCom,
                            AppType.USER);
                        break;
                    default:
                        break;
                }
            }
            catch
            {
            }
        }

        private async Task FireNotificationCreateServiceOrder(DigitalServiceOrder oDigitalServiceOrder, string serviceName, UserIdentifier[] admins)
        {
            try
            {
                string detailUrlApp = $"yooioc://app/digital-service-order/detail?id={oDigitalServiceOrder.Id}";
                var messageAccept = new UserMessageNotificationDataBase(
                    AppNotificationAction.DigitalServiceOrder,
                    AppNotificationIcon.DigitalServiceOrder,
                    TypeAction.Detail,
                    $"Dịch vụ {serviceName} có một đơn đặt mới. Nhấn để xem chi tiết !",
                    detailUrlApp,
                    "",
                    "",
                    "",
                    oDigitalServiceOrder.Id

                );
                await _appNotifier.SendMessageNotificationInternalAsync(
                    "Yoolife dịch vụ nội khu !",
                    $"Dịch vụ {serviceName} có một đơn đặt mới. Nhấn để xem chi tiết !",
                    detailUrlApp,
                    "",
                    admins,
                    messageAccept,
                    AppType.IOC);
            }
            catch
            {
            }
        }
    }
}
