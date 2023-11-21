using Abp.Application.Services;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using IMAX.App.ServiceHttpClient.Dto.IMAX.SmartCommunity.WorkDtos;
using IMAX.App.ServiceHttpClient.Dto;
using IMAX.Application;
using IMAX.Authorization.Users;
using IMAX.Common.DataResult;
using IMAX.EntityDb;
using Microsoft.EntityFrameworkCore;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twilio.Rest.Api.V2010.Account;
using static IMAX.Common.Enum.CommonENum;
using IMAX.App.ServiceHttpClient.IMAX.SmartCommunity;
using IMAX.Authorization;
using IMAX.QueriesExtension;
using IMAX.Notifications;
using Abp;

namespace IMAX.Services
{
    public interface IDigitalServiceOrderAppService : IApplicationService
    {
        Task<DataResult> GetAllAsync(GetAllDigitalServiceOrderInputDto input);
        Task<DataResult> GetById(long id);
        Task<DataResult> Create(DigitalServiceOrderCrearteDto input);
        Task<DataResult> Update(DigitalServiceOrderCrearteDto input);
        Task<DataResult> UpdateState(UpdateStateDigitalServiceOrderDto input);
        Task<DataResult> UpdateRate(UpdateRateDigitalServiceOrderDto input);
        Task<DataResult> UpdateFeedback(UpdateFeedbackDigitalServiceOrderDto input);
        Task<DataResult> Delete(long id);
    }
    [AbpAuthorize]
    public class DigitalServiceOrderAppService : IMAXAppServiceBase, IDigitalServiceOrderAppService
    {
        private readonly IRepository<DigitalServiceOrder, long> _repository;
        private readonly IRepository<DigitalServices, long> _digitalServicesRepository;
        private readonly IRepository<Citizen, long> _citizenRepos;
        private readonly IHttpWorkAssignmentService _httpWorkAssignmentService;
        private readonly IAppNotifier _appNotifier;
        public DigitalServiceOrderAppService(IRepository<DigitalServiceOrder, long> repository,
IRepository<DigitalServices, long> digitalServicesRepository, IRepository<Citizen, long> citizenRepos, IHttpWorkAssignmentService httpWorkAssignmentService, IAppNotifier appNotifier)
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
                                                            })
                    .WhereIf(!string.IsNullOrEmpty(input.Keyword), x =>
x.Address.ToLower().Contains(input.Keyword.ToLower()))
                    .WhereByBuildingOrUrbanIf(!IsGranted(PermissionNames.Data_Admin), buIds)
    .WhereIf(input.Status > 0, x => x.Status == input.Status)
    .WhereIf(input.StatusTab > 0, x => input.StatusTab == 1? x.Status == (int)TypeActionUpdateStateServiceOrder.CREATE: input.StatusTab == 2? (x.Status > (int)TypeActionUpdateStateServiceOrder.CREATE && x.Status < (int)TypeActionUpdateStateServiceOrder.COMPLETE) : x.Status > (int)TypeActionUpdateStateServiceOrder.FEEDBACK)
    .WhereIf(input.ServiceId > 0, x => x.ServiceId == input.ServiceId)
;
                List<DigitalServiceOrderDto> result = await query.ApplySort(input.OrderBy, (SortBy)input.SortBy).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
                return DataResult.ResultSuccess(result, Common.Resource.QuanLyChung.GetAllSuccess, query.Count());
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
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
               
                 try //để tạm trong trường hợp trên máy local ko chạy được grpc
                {
                    MicroserviceResultDto<List<WorkDto>> result = await _httpWorkAssignmentService.GetAllWorkByRelatedId(new GetListWorkByRelatedIdDto()
                    {
                        RelatedId = id,
                        RelationshipType = WorkAssociationType.DIGITAL_SERVICES,
                    });
                    if (result != null)
                    {
                        data.WorkOrder = result.Result;
                    }
                }
                catch (Exception e)
                {

                }
              
                return DataResult.ResultSuccess(data, Common.Resource.QuanLyChung.Success);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }
        public async Task<DataResult> Create(DigitalServiceOrderCrearteDto dto)
        {
            try
            {
                DigitalServiceOrder item = dto.MapTo<DigitalServiceOrderCrearteDto>();
                if (dto.ArrServiceDetails != null)
                    item.ServiceDetails = JsonConvert.SerializeObject(dto.ArrServiceDetails);
                item.TenantId = AbpSession.TenantId;
                await _repository.InsertAsync(item);
                return DataResult.ResultSuccess(Common.Resource.QuanLyChung.InsertSuccess);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }
        public async Task<DataResult> Update(DigitalServiceOrderCrearteDto dto)
        {
            try
            {
                DigitalServiceOrder item = await _repository.GetAsync(dto.Id);
                if (item != null)
                {
                    dto.MapTo(item);
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
                throw new UserFriendlyException(e.Message);
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
                throw new UserFriendlyException(e.Message);
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
                throw new UserFriendlyException(e.Message);
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
                throw new UserFriendlyException(e.Message);
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
                throw new UserFriendlyException(e.Message);
            }
        }
        private async Task FireNotificationStateServiceOrder(DigitalServiceOrder oDigitalServiceOrder)
        {
            try
            {
                string detailUrlApp;
                switch (oDigitalServiceOrder.Status)
                {
                    case (int)TypeActionUpdateStateServiceOrder.RECEIVE:
                        detailUrlApp = "yoolife://app/digital-service-order";
                        var messageAccept = new UserMessageNotificationDataBase(
                            AppNotificationAction.DigitalServiceOrder,
                            AppNotificationIcon.DigitalServiceOrder,
                            TypeAction.Detail,
                            "Dịch vụ bạn đặt đã được tiếp nhận",
                            detailUrlApp,
                            "",
                            "",
                            "",
                            oDigitalServiceOrder.Id

                        );
                        await _appNotifier.SendUserMessageNotifyFullyAsync(
                            "Thông báo đặt dịch vụ",
                            "Dịch vụ bạn đặt đã được tiếp nhận!",
                            detailUrlApp,
                            "",
                            new UserIdentifier[] { new UserIdentifier(oDigitalServiceOrder.TenantId, oDigitalServiceOrder.CreatorUserId.Value) },
                            messageAccept);
                        break;
                    case (int)TypeActionUpdateStateServiceOrder.FEEDBACK:
                        detailUrlApp = "yoolife://app/digital-service-order";
                        var messageAcceptFeed = new UserMessageNotificationDataBase(
                             AppNotificationAction.DigitalServiceOrder,
                            AppNotificationIcon.DigitalServiceOrder,
                            TypeAction.Detail,
                            "Dịch vụ bạn đặt có phản hồi",
                            detailUrlApp,
                            "",
                            "",
                            "",
                            0

                        );
                        await _appNotifier.SendUserMessageNotifyFullyAsync(
                            "Thông báo đặt dịch vụ",
                            "Dịch vụ bạn đặt có phản hồi. Hãy xác nhận!",
                            detailUrlApp,
                            "",
                            new UserIdentifier[] { new UserIdentifier(oDigitalServiceOrder.TenantId, oDigitalServiceOrder.CreatorUserId.Value) },
                            messageAcceptFeed);
                        break;
                    case (int)TypeActionUpdateStateServiceOrder.COMPLETE:
                        detailUrlApp = "yoolife://app/digital-service-order";
                        var messageAcceptCom = new UserMessageNotificationDataBase(
                             AppNotificationAction.DigitalServiceOrder,
                            AppNotificationIcon.DigitalServiceOrder,
                            TypeAction.Detail,
                            "Dịch vụ bạn đặt đã hoàn thành. Hãy thanh toán!",
                            detailUrlApp,
                            "",
                            "",
                            "",
                            0

                        );
                        await _appNotifier.SendUserMessageNotifyFullyAsync(
                            "Thông báo đặt dịch vụ",
                            "Dịch vụ bạn đặt đã hoàn thành. Hãy thanh toán!",
                            detailUrlApp,
                            "",
                            new UserIdentifier[] { new UserIdentifier(oDigitalServiceOrder.TenantId, oDigitalServiceOrder.CreatorUserId.Value) },
                            messageAcceptCom);
                        break;
                    default:
                        break;
                }
            }
            catch
            {
            }
        }
    }
}
