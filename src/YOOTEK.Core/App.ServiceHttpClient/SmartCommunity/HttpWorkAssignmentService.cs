using Abp;
using Abp.Application.Services.Dto;
using Abp.Runtime.Session;
using Yootek.App.ServiceHttpClient.Dto;
using Yootek.App.ServiceHttpClient.Dto.Yootek.SmartCommunity.WorkDtos;
using Yootek.Notifications;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Yootek.EntityDb;
using Yootek.Authorization.Users;
using System.Linq;

namespace Yootek.App.ServiceHttpClient.Yootek.SmartCommunity
{
    public interface IHttpWorkAssignmentService
    {
        #region IWorkService
        Task<MicroserviceResultDto<WorkStatisticDto>> GetWorkStatistic(GetWorkStatisticDto input);
        Task<MicroserviceResultDto<Dictionary<string, int>>> GetWorkStatisticGeneral(GetWorkStatisticGeneralDto input);
        Task<MicroserviceResultDto<PagedResultDto<WorkDto>>> AdminGetListWork(GetListWorkDto input);
        Task<MicroserviceResultDto<WorkDetailDto>> AdminGetWorkById(long id);
        Task<MicroserviceResultDto<PagedResultDto<WorkDto>>> GetListWork(GetListWorkDto input);
        Task<MicroserviceResultDto<PagedResultDto<GetAllWorksPlanDto>>> GetListWorkPlan(GetAllWorksPlanQuery input);
        Task<MicroserviceResultDto<List<WorkDto>>> GetAllWorkByRelatedId(GetListWorkByRelatedIdDto input);
        Task<MicroserviceResultDto<WorkDetailDto>> GetWorkById(long id);
        Task<MicroserviceResultDto<WorkDetailDto>> GetWorkByQrCode(string qrCode);
        Task<MicroserviceResultDto<long?>> CreateWork(CreateWorkDto input);
        Task<MicroserviceResultDto<bool>> UpdateWork(UpdateWorkDto input);
        Task<MicroserviceResultDto<bool>> AssignWork(AssignWorkDto input);
        Task<MicroserviceResultDto<List<UpdateStateRelateDto>>> UpdateStateWork(UpdateStateWorkDto input);
        Task<MicroserviceResultDto<bool>> DeleteWork(DeleteWorkDto input);
        Task<MicroserviceResultDto<bool>> DeleteManyWork(DeleteManyWorkDto input);
        Task<MicroserviceResultDto<PagedResultDto<GetAllWorksNotifyDto>>> GetListWorkNotìy(GetAllWorksNotifyQuery input);
        #endregion

        #region IWorkTypeService
        Task<MicroserviceResultDto<PagedResultDto<WorkTypeDto>>> GetListWorkType(GetAllWorkTypeDto input);
        Task<MicroserviceResultDto<List<WorkTypeDto>>> GetListWorkTypeNotPaging(GetAllWorkTypeNotPagingDto input);
        Task<MicroserviceResultDto<WorkTypeDetailDto>> GetWorkType(GetWorkTypeDto input);
        Task<MicroserviceResultDto<long>> CreateWorkType(CreateWorkTypeDto input);
        Task<MicroserviceResultDto<bool>> UpdateWorkType(UpdateWorkTypeDto input);
        Task<MicroserviceResultDto<bool>> DeleteWorkType(DeleteWorkTypeDto input);
        #endregion

        #region IWorkDetailService
        Task<MicroserviceResultDto<PagedResultDto<DetailWorkDto>>> GetListWorkDetail(GetAllWorkDetailDto input);
        Task<MicroserviceResultDto<List<DetailWorkDto>>> GetListWorkDetailNotPaging(GetAllWorkDetailNotPagingDto input);
        Task<MicroserviceResultDto<bool>> CreateWorkDetail(CreateWorkDetailDto input);
        Task<MicroserviceResultDto<bool>> UpdateWorkDetail(UpdateWorkDetailDto input);
        Task<MicroserviceResultDto<bool>> DeleteWorkDetail(DeleteWorkDetailDto input);
        #endregion

        #region IWorkLogTime 
        Task<MicroserviceResultDto<PagedResultDto<WorkLogTimeDto>>> GetListWorkLogTime(GetAllWorkLogTimeDto input);
        Task<MicroserviceResultDto<PagedResultDto<GetWorkTimeSheetDto>>> GetListWorkLogTimeSheet(GetAllWorkTimeSheetQuery input);
        Task<MicroserviceResultDto<WorkLogTimeDto>> GetWorkLogTimeById(GetWorkLogTimeByIdDto input);
        Task<MicroserviceResultDto<bool>> CreateWorkLogTime(CreateWorkLogTimeDto input);
        Task<MicroserviceResultDto<bool>> CreateManyWorkLogTime(CreateManyWorkLogTimeDto input);
        Task<MicroserviceResultDto<bool>> UpdateManyWorkLogTime(UpdateManyWorkLogTimeDto input);
        Task<MicroserviceResultDto<bool>> UpdateWorkLogTime(UpdateWorkLogTimeDto input);
        Task<MicroserviceResultDto<bool>> UpdateStatusWorkLogTime(UpdateStatusWorkLogTimeDto input);
        Task<MicroserviceResultDto<bool>> UpdateStatusLogTime(UpdateStatusLogTimeDto input);
        Task<MicroserviceResultDto<bool>> DeleteWorkLogTime(DeleteWorkLogTimeDto input);
        #endregion

        #region IWorkHistory 
        Task<MicroserviceResultDto<PagedResultDto<WorkHistoryDto>>> GetListWorkHistory(GetAllWorkHistoryDto input);
        Task<MicroserviceResultDto<WorkHistoryDto>> GetWorkHistoryById(GetWorkHistoryByIdDto input);
        Task<MicroserviceResultDto<bool>> CreateWorkHistory(CreateWorkHistoryDto input);
        Task<MicroserviceResultDto<bool>> UpdateWorkHistory(UpdateWorkHistoryDto input);
        Task<MicroserviceResultDto<bool>> DeleteWorkHistory(DeleteWorkHistoryDto input);
        #endregion

        #region IWorkCommentService
        Task<MicroserviceResultDto<PagedResultDto<WorkCommentDto>>> GetListWorkComment(GetAllWorkCommentDto input);
        Task<MicroserviceResultDto<List<WorkCommentDto>>> GetListWorkCommentNotPaging(GetAllWorkCommentNotPagingDto input);
        Task<MicroserviceResultDto<WorkCommentDetailDto>> GetWorkComment(GetWorkCommentDto input);
        Task<MicroserviceResultDto<bool>> CreateWorkComment(CreateWorkCommentDto input);
        Task<MicroserviceResultDto<bool>> UpdateWorkComment(UpdateWorkCommentDto input);
        Task<MicroserviceResultDto<bool>> DeleteWorkComment(DeleteWorkCommentDto input);
        #endregion

        #region IWorkTurnService
        Task<MicroserviceResultDto<PagedResultDto<WorkTurnDto>>> GetListWorkTurn(GetAllWorkTurnsDto input);
        Task<MicroserviceResultDto<PagedResultDto<WorkTurnDto>>> GetListWorkTurnNotPaging(GetAllWorkTurnsDto input);
        Task<MicroserviceResultDto<WorkTurnDto>> GetWorkTurnById(GetWorkTurnByIdDto input);
        Task<MicroserviceResultDto<bool>> CreateWorkTurn(CreateWorkTurnDto input);
        Task<MicroserviceResultDto<bool>> UpdateWorkTurn(UpdateWorkTurnDto input);
        Task<MicroserviceResultDto<bool>> DeleteWorkTurn(DeleteWorkTurnDto input);
        Task<MicroserviceResultDto<bool>> DeleteManyWorkTurn(DeleteManyWorkTurnDto input);
        #endregion
    }
    public class HttpWorkAssignmentService : IHttpWorkAssignmentService
    {
        private readonly HttpClient _client;
        private readonly IAbpSession _session;
        private readonly IAppNotifier _appNotifier;

        public HttpWorkAssignmentService(HttpClient client, IAbpSession session, IAppNotifier appNotifier)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _appNotifier = appNotifier ?? throw new ArgumentNullException(nameof(session));
        }

        #region WorkService
        public async Task<MicroserviceResultDto<WorkStatisticDto>> GetWorkStatistic(GetWorkStatisticDto input)
        {
            var query = "api/v1/work/admin/get-work-statistic" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);

            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<WorkStatisticDto>>();
        }
        public async Task<MicroserviceResultDto<Dictionary<string, int>>> GetWorkStatisticGeneral(GetWorkStatisticGeneralDto input)
        {
            var query = "api/v1/work/admin/get-work-statistic-general" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);

            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<Dictionary<string, int>>>();
        }
        public async Task<MicroserviceResultDto<PagedResultDto<WorkDto>>> AdminGetListWork(GetListWorkDto input)
        {
            var query = "api/v1/work/admin/get-list-work" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);

            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<PagedResultDto<WorkDto>>>();
        }
        public async Task<MicroserviceResultDto<PagedResultDto<GetAllWorksNotifyDto>>> GetListWorkNotìy(GetAllWorksNotifyQuery input)
        {
            var query = "api/v1/work/get-list-work-notify" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);

            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<PagedResultDto<GetAllWorksNotifyDto>>>();
        }
        public async Task<MicroserviceResultDto<WorkDetailDto>> AdminGetWorkById(long id)
        {
            var query = "api/v1/work/admin/get-work-by-id?id=" + id;
            using var request = new HttpRequestMessage(HttpMethod.Get, query);

            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<WorkDetailDto>>();
        }
        public async Task<MicroserviceResultDto<PagedResultDto<WorkDto>>> GetListWork(GetListWorkDto input)
        {
            var query = "api/v1/work/get-list-work" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);

            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<PagedResultDto<WorkDto>>>();
        }
        public async Task<MicroserviceResultDto<PagedResultDto<GetAllWorksPlanDto>>> GetListWorkPlan(GetAllWorksPlanQuery input)
        {
            var query = "api/v1/work/get-work-plan" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);

            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<PagedResultDto<GetAllWorksPlanDto>>>();
        }
        public async Task<MicroserviceResultDto<List<WorkDto>>> GetAllWorkByRelatedId(GetListWorkByRelatedIdDto input)
        {
            var query = "api/v1/work/get-list-work-by-relatedId" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);

            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<List<WorkDto>>>();
        }
        public async Task<MicroserviceResultDto<WorkDetailDto>> GetWorkById(long id)
        {
            var query = "api/v1/work/get-work-by-id?id=" + id;
            using var request = new HttpRequestMessage(HttpMethod.Get, query);

            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<WorkDetailDto>>();
        }
        public async Task<MicroserviceResultDto<WorkDetailDto>> GetWorkByQrCode(string qrCode)
        {
            var query = "api/v1/work/get-work-by-qrcode?qrcode=" + qrCode;
            using var request = new HttpRequestMessage(HttpMethod.Get, query);

            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<WorkDetailDto>>();
        }
        //public async Task<MicroserviceResultDto<bool>> CreateWork(CreateWorkDto input)
        //{
        //    using (var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/work/create-work"))
        //    {
        //        request.HandlePostAsJson(input, _session);
        //        var response = await _client.SendAsync(request);
        //        return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        //    }
        //}
        public async Task<MicroserviceResultDto<long?>> CreateWork(CreateWorkDto input)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/work/create-work"))
            {
                request.HandlePostAsJson(input, _session);
                var response = await _client.SendAsync(request);
                var result = await response.ReadContentAs<MicroserviceResultDto<long?>>();
                if (result?.Success == true)
                {
                    try
                    {
                        // Sau khi công việc đã được tạo, gửi thông báo cho RecipientIds và SupervisorIds
                        await SendWorkNotification(result.Result.Value, input.RecipientIds, input.SupervisorIds, $"yooioc://app/work/detail?id={result.Result.Value}", $"/tasks?id={result.Result.Value}&tenantId={_session.TenantId}");
                    }
                    catch
                    {
                        Console.WriteLine("Send nofitication fail");
                    }
                }
                return result;
            }
        }

        public async Task<MicroserviceResultDto<bool>> AssignWork(AssignWorkDto input)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/work/assign-work");

            request.HandlePutAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        public async Task<MicroserviceResultDto<bool>> UpdateWork(UpdateWorkDto input)
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/work/update-work");

            request.HandlePutAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        public async Task<MicroserviceResultDto<List<UpdateStateRelateDto>>> UpdateStateWork(UpdateStateWorkDto input)
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/work/update-state-work");

            request.HandlePutAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<List<UpdateStateRelateDto>>>();
        }
        public async Task<MicroserviceResultDto<bool>> DeleteWork(DeleteWorkDto input)
        {
            var query = "api/v1/work/delete-work" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Delete, query);

            request.HandleDeleteAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        public async Task<MicroserviceResultDto<bool>> DeleteManyWork(DeleteManyWorkDto input)
        {
            var query = "api/v1/work/delete-many-work" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Delete, query);

            request.HandleDeleteAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        #endregion

        #region WorkTypeService
        public async Task<MicroserviceResultDto<PagedResultDto<WorkTypeDto>>> GetListWorkType(GetAllWorkTypeDto input)
        {
            var query = "api/v1/WorkType/get-all-work-type" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);

            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<PagedResultDto<WorkTypeDto>>>();
        }
        public async Task<MicroserviceResultDto<List<WorkTypeDto>>> GetListWorkTypeNotPaging(GetAllWorkTypeNotPagingDto input)
        {
            var query = "api/v1/WorkType/get-all-work-type-not-paging" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);

            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<List<WorkTypeDto>>>();
        }
        public async Task<MicroserviceResultDto<WorkTypeDetailDto>> GetWorkType(GetWorkTypeDto input)
        {
            var query = "api/v1/WorkType/get-work-type" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);

            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<WorkTypeDetailDto>>();
        }

        public async Task<MicroserviceResultDto<long>> CreateWorkType(CreateWorkTypeDto input)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/WorkType/create-work-type");

            request.HandlePostAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<long>>();
        }

        public async Task<MicroserviceResultDto<bool>> UpdateWorkType(UpdateWorkTypeDto input)
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/WorkType/update-work-type");

            request.HandlePutAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }

        public async Task<MicroserviceResultDto<bool>> DeleteWorkType(DeleteWorkTypeDto input)
        {
            var query = "api/v1/WorkType/delete-work-type" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Delete, query);

            request.HandleDeleteAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        #endregion

        #region WorkDetailService
        public async Task<MicroserviceResultDto<PagedResultDto<DetailWorkDto>>> GetListWorkDetail(GetAllWorkDetailDto input)
        {
            var query = "api/v1/WorkDetail/get-all-work-detail" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);

            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<PagedResultDto<DetailWorkDto>>>();
        }
        public async Task<MicroserviceResultDto<List<DetailWorkDto>>> GetListWorkDetailNotPaging(GetAllWorkDetailNotPagingDto input)
        {
            var query = "api/v1/WorkDetail/get-all-work-detail-not-paging" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);

            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<List<DetailWorkDto>>>();
        }
        public async Task<MicroserviceResultDto<bool>> CreateWorkDetail(CreateWorkDetailDto input)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/WorkDetail/create-work-detail");

            request.HandlePostAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }

        public async Task<MicroserviceResultDto<bool>> UpdateWorkDetail(UpdateWorkDetailDto input)
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/WorkDetail/update-work-detail");

            request.HandlePutAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }

        public async Task<MicroserviceResultDto<bool>> DeleteWorkDetail(DeleteWorkDetailDto input)
        {
            var query = "api/v1/WorkDetail/delete-work-detail" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Delete, query);

            request.HandleDeleteAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        #endregion

        #region WorkLogTimeService
        public async Task<MicroserviceResultDto<PagedResultDto<WorkLogTimeDto>>> GetListWorkLogTime(GetAllWorkLogTimeDto input)
        {
            var query = "api/v1/WorkLogTime/get-list-work-logtime" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);

            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<PagedResultDto<WorkLogTimeDto>>>();
        }
        public async Task<MicroserviceResultDto<PagedResultDto<GetWorkTimeSheetDto>>> GetListWorkLogTimeSheet(GetAllWorkTimeSheetQuery input)
        {
            var query = "api/v1/WorkLogTime/get-list-work-time-sheet" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);

            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<PagedResultDto<GetWorkTimeSheetDto>>>();
        }
        public async Task<MicroserviceResultDto<WorkLogTimeDto>> GetWorkLogTimeById(GetWorkLogTimeByIdDto input)
        {
            var query = "api/v1/WorkLogTime/get-work-logtime-detail" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);

            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<WorkLogTimeDto>>();
        }
        public async Task<MicroserviceResultDto<bool>> CreateWorkLogTime(CreateWorkLogTimeDto input)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/WorkLogTime/create-work-logtime");

            request.HandlePostAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        public async Task<MicroserviceResultDto<bool>> CreateManyWorkLogTime(CreateManyWorkLogTimeDto input)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/WorkLogTime/create-many-work-logtime");

            request.HandlePostAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        public async Task<MicroserviceResultDto<bool>> UpdateWorkLogTime(UpdateWorkLogTimeDto input)
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/WorkLogTime/update-work-logtime");

            request.HandlePutAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        public async Task<MicroserviceResultDto<bool>> UpdateStatusWorkLogTime(UpdateStatusWorkLogTimeDto input)
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/WorkLogTime/update-status-work-logtime");

            request.HandlePutAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        public async Task<MicroserviceResultDto<bool>> UpdateStatusLogTime(UpdateStatusLogTimeDto input)
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/WorkLogTime/update-status-logtime");

            request.HandlePutAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        public async Task<MicroserviceResultDto<bool>> UpdateManyWorkLogTime(UpdateManyWorkLogTimeDto input)
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/WorkLogTime/update-many-work-logtime");

            request.HandlePutAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        public async Task<MicroserviceResultDto<bool>> DeleteWorkLogTime(DeleteWorkLogTimeDto input)
        {
            var query = "api/v1/WorkLogTime/delete-work-logtime" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Delete, query);

            request.HandleDeleteAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        #endregion

        #region WorkCommentService
        public async Task<MicroserviceResultDto<PagedResultDto<WorkCommentDto>>> GetListWorkComment(GetAllWorkCommentDto input)
        {
            var query = "api/v1/WorkComment/get-all-work-comment" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);

            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<PagedResultDto<WorkCommentDto>>>();
        }
        public async Task<MicroserviceResultDto<List<WorkCommentDto>>> GetListWorkCommentNotPaging(GetAllWorkCommentNotPagingDto input)
        {
            var query = "api/v1/WorkComment/get-all-work-comment-not-paging" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);

            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<List<WorkCommentDto>>>();
        }
        public async Task<MicroserviceResultDto<WorkCommentDetailDto>> GetWorkComment(GetWorkCommentDto input)
        {
            var query = "api/v1/WorkComment/get-work-comment" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);

            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<WorkCommentDetailDto>>();
        }

        public async Task<MicroserviceResultDto<bool>> CreateWorkComment(CreateWorkCommentDto input)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/WorkComment/create-work-comment");

            request.HandlePostAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }

        public async Task<MicroserviceResultDto<bool>> UpdateWorkComment(UpdateWorkCommentDto input)
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/WorkComment/update-work-comment");

            request.HandlePutAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }

        public async Task<MicroserviceResultDto<bool>> DeleteWorkComment(DeleteWorkCommentDto input)
        {
            var query = "api/v1/WorkComment/delete-work-comment" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Delete, query);

            request.HandleDeleteAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        #endregion

        #region WorkHistoryService
        public async Task<MicroserviceResultDto<PagedResultDto<WorkHistoryDto>>> GetListWorkHistory(GetAllWorkHistoryDto input)
        {
            var query = "api/v1/WorkHistory/get-list-work-history" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);

            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<PagedResultDto<WorkHistoryDto>>>();
        }
        public async Task<MicroserviceResultDto<WorkHistoryDto>> GetWorkHistoryById(GetWorkHistoryByIdDto input)
        {
            var query = "api/v1/WorkHistory/get-work-history-detail" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);

            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<WorkHistoryDto>>();
        }
        public async Task<MicroserviceResultDto<bool>> CreateWorkHistory(CreateWorkHistoryDto input)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/WorkHistory/create-work-history");

            request.HandlePostAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        public async Task<MicroserviceResultDto<bool>> UpdateWorkHistory(UpdateWorkHistoryDto input)
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/WorkHistory/update-work-history");

            request.HandlePutAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        public async Task<MicroserviceResultDto<bool>> DeleteWorkHistory(DeleteWorkHistoryDto input)
        {
            var query = "api/v1/WorkHistory/delete-work-history" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Delete, query);

            request.HandleDeleteAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        #endregion

        #region WorkTurnService
        public async Task<MicroserviceResultDto<PagedResultDto<WorkTurnDto>>> GetListWorkTurn(GetAllWorkTurnsDto input)
        {
            var query = "api/v1/WorkTurn/get-list-work-turn" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);

            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<PagedResultDto<WorkTurnDto>>>();
        }
        public async Task<MicroserviceResultDto<PagedResultDto<WorkTurnDto>>> GetListWorkTurnNotPaging(GetAllWorkTurnsDto input)
        {
            var query = "api/v1/WorkTurn/get-list-work-turn-not-paging" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);

            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<PagedResultDto<WorkTurnDto>>>();
        }
        public async Task<MicroserviceResultDto<WorkTurnDto>> GetWorkTurnById(GetWorkTurnByIdDto input)
        {
            var query = "api/v1/WorkTurn/get-work-turn-by-id" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);

            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<WorkTurnDto>>();
        }

        public async Task<MicroserviceResultDto<bool>> CreateWorkTurn(CreateWorkTurnDto input)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/WorkTurn/create-work-turn");

            request.HandlePostAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }

        public async Task<MicroserviceResultDto<bool>> UpdateWorkTurn(UpdateWorkTurnDto input)
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/WorkTurn/update-work-turn");

            request.HandlePutAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }

        public async Task<MicroserviceResultDto<bool>> DeleteWorkTurn(DeleteWorkTurnDto input)
        {
            var query = "api/v1/WorkTurn/delete-work-turn" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Delete, query);

            request.HandleDeleteAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        public async Task<MicroserviceResultDto<bool>> DeleteManyWorkTurn(DeleteManyWorkTurnDto input)
        {
            var query = "api/v1/work/delete-many-work-turn" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Delete, query);

            request.HandleDeleteAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        #endregion

        #region helpers method 
        private async Task SendWorkNotification(long workId, List<long> recipientIds, List<long> supervisorIds, string detailUrlApp, string detailUrlWA)
        {
            var message = "Thông báo quản lý công việc !";
            var notification = new NotificationWithContentIdDatabase(
                workId,
                AppNotificationAction.WorkNotification,
                AppNotificationIcon.WorkNotificationIcon,
                TypeAction.Detail,
                "Bạn vừa được giao công việc mới. Nhấn để xem chi tiết!",
                detailUrlApp,
                detailUrlWA,
                "",
            ""
            );

            var recipients = recipientIds.Select(x => new UserIdentifier(_session.TenantId, x)).ToList();

            await _appNotifier.SendMessageNotificationInternalAsync(
                message,
                "Bạn vừa được giao công việc mới. Nhấn để xem chi tiết!",
                detailUrlApp,
                detailUrlWA,
                recipients.ToArray(),
                notification,
                AppType.IOC
            );

            var supervisors = supervisorIds.Select(x => new UserIdentifier(_session.TenantId, x)).ToList();
            await _appNotifier.SendMessageNotificationInternalAsync(
                message,
                "Bạn vừa được giao công việc mới. Nhấn để xem chi tiết!",
                detailUrlApp,
                detailUrlWA,
                supervisors.ToArray(),
                notification,
                AppType.IOC
            );
        }

        #endregion
    }
}
