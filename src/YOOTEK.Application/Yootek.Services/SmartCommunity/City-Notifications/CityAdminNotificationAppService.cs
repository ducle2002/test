using Abp;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.MultiTenancy;
using Abp.Organizations;
using Abp.RealTime;
using Yootek.ApbCore.Data;
using Yootek.Authorization;
using Yootek.Authorization.Users;
using Yootek.Common.DataResult;
using Yootek.Common.Enum;
using Yootek.EntityDb;
using Yootek.Notifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yootek.Organizations;
using Abp.Linq.Extensions;
using Abp.UI;
using static Yootek.Common.Enum.CommonENum;
using Yootek.Application;
using Yootek.QueriesExtension;

namespace Yootek.Services
{
    public interface ICityAdminNotificationAppService : IApplicationService
    {
        Task<object> GetAllNotificationUserTenant(NotificationInput input);

        Task<object> GetAllNotificationUser();
        Task<object> CreatOrUpdateCommentAsync(CommentDto input);
        Task<object> CreatOrUpdateFollowAsync(CommentDto input);
        Task<object> CreatOrUpdateLikeAsync(LikeDto input);
        Task<object> GetAllCommentAsync(GetCommentInput input);
        Task<object> CreateOrUpdateNotification(CityNotificationDto input);
        Task<object> DeleteNotification(long id);
        Task<object> DeleteCommentAsync(long commentId);

    }

    //[AbpAuthorize(PermissionNames.Pages_Operations, PermissionNames.Pages_Digitals_Notifications, PermissionNames.Pages_Government, PermissionNames.Pages_Government_Digital_Notices)]
    //Pages_Operations_Citizens_CityNotification
    // [AbpAuthorize(PermissionNames.Pages_Operations, PermissionNames.Pages_Operations_Digital_Notices)]
    [AbpAuthorize]
    public class CityAdminNotificationAppService : YootekAppServiceBase, ICityAdminNotificationAppService
    {
        private readonly IRepository<CityNotification, long> _cityNotificationRepos;
        private readonly IRepository<CityNotificationComment, long> _commentRepos;
        private readonly IRepository<CitizenReflectLike, long> _userFeedbackLikeRepos;
        private readonly IRepository<User, long> _userRepos;
        private readonly IRepository<UserCityNotification, long> _userCityNotificationRepos; //luu cu dan nhan thong bao
        private readonly IRepository<Citizen, long> _citizenRepos;
        private readonly IRepository<Apartment, long> _apartmentRepos;
        private readonly IRepository<AppOrganizationUnit, long> _appOrganizationUnitRepository;
        private readonly IRepository<UserOrganizationUnit, long> _userOrganizationUnitRepository;
        //   private readonly IFeedbackListExcelExporter _feedbackListExcelExporter;
        private readonly ISqlExecuter _sqlExecute;
        private readonly IAppNotifier _appNotifier;

        public CityAdminNotificationAppService(
            IRepository<CityNotification, long> cityNotificationRepos,
            IRepository<CityNotificationComment, long> commentRepos,
            IRepository<User, long> userRepos,
            IRepository<CitizenReflectLike, long> userFeedbackLikeRepos,
            IRepository<UserCityNotification, long> userCityNotificationRepos,

            IRepository<AppOrganizationUnit, long> appOrganizationUnitRepository,
            IRepository<UserOrganizationUnit, long> userOrganizationUnitRepository,

            IAppNotifier appNotifier,
            IRepository<Citizen, long> citizenRepos,
            IRepository<Apartment, long> apartmentRepos,
        //     IFeedbackListExcelExporter feedbackListExcelExporter,
            IRepository<UserOrganizationUnit, long> userOrganizationRepos,
            ISqlExecuter sqlExecute
            )
        {
            _cityNotificationRepos = cityNotificationRepos;
            _commentRepos = commentRepos;
            _userFeedbackLikeRepos = userFeedbackLikeRepos;
            _userCityNotificationRepos = userCityNotificationRepos;
            _userRepos = userRepos;
            _citizenRepos = citizenRepos;
            _apartmentRepos = apartmentRepos;
            //_feedbackListExcelExporter = feedbackListExcelExporter;
            _appOrganizationUnitRepository = appOrganizationUnitRepository;
            _userOrganizationUnitRepository = userOrganizationUnitRepository;
            _sqlExecute = sqlExecute;
            _appNotifier = appNotifier;
        }

        protected IQueryable<CityNotificationDto> QueryDataCityNotification(NotificationInput input)
        {

            var query = (from noti in _cityNotificationRepos.GetAll()
                             //join cm in _commentRepos.GetAll() on noti.Id equals cm.CityNotificationId into tb_cm
                             //from cm in tb_cm.DefaultIfEmpty()
                         join ou in _appOrganizationUnitRepository.GetAll() on noti.OrganizationUnitId equals ou.Id into tb_ou
                         from ou in tb_ou.DefaultIfEmpty()
                         where noti.Type == input.Type
                         select new CityNotificationDto()
                         {
                             CreationTime = noti.CreationTime,
                             // CreatorUserId = noti.CreatorUserId,
                             Data = noti.Data,
                             FileUrl = noti.FileUrl,
                             // Follow = noti.Follow,
                             Id = noti.Id,
                             Name = noti.Name,
                             Type = noti.Type,
                             OrganizationUnitId = noti.OrganizationUnitId,
                             CountComment = (from com in _commentRepos.GetAll()
                                             where com.CityNotificationId == noti.Id
                                             && !com.IsLike.HasValue
                                             select com).AsQueryable().Count(),
                             //CountFollow = (from com in _commentRepos.GetAll()
                             //               where com.CityNotificationId == noti.Id
                             //               && com.IsLike == true
                             //               select com).AsQueryable().Count(),
                             ReceiverGroupCode = noti.IsReceiveAll == null || noti.IsReceiveAll == true ? null : noti.ReceiverGroupCode,
                             State = noti.State,
                             IsAllowComment = noti.IsAllowComment,
                             ReceiveAll = noti.ReceiveAll,
                             BuildingId = noti.BuildingId,
                             AttachUrls = noti.AttachUrls,
                             UrbanId = noti.UrbanId ?? (from ou in _appOrganizationUnitRepository.GetAll()
                                                        where input.BuildingId == ou.Id
                                                        select ou.ParentId).FirstOrDefault() ?? noti.UrbanId
                         }).AsQueryable();

            return query;
        }

        public async Task<object> GetAllNotificationUserTenant(NotificationInput input)
        {
            try
            {
                List<long> buIds = UserManager.GetAccessibleBuildingOrUrbanIds();
                var query = QueryDataCityNotification(input)
                .WhereByBuildingOrUrbanIf(!IsGranted(IOCPermissionNames.Data_Admin), buIds)
                  .WhereIf(input.ReceiverGroupCode != null, x => input.ReceiverGroupCode == x.ReceiverGroupCode)
                  .WhereIf(input.OrganizationUnitId != null, x => input.OrganizationUnitId == x.OrganizationUnitId)
                  .WhereIf(input.ReceiveAll != null, x => input.ReceiveAll == x.ReceiveAll)
                  .WhereIf(input.BuildingId != null, x => input.BuildingId == x.BuildingId)
                  .WhereIf(input.UrbanId != null, x => input.UrbanId == x.UrbanId)
                  .WhereIf(input.Type != null, x => input.Type == x.Type)
                  //.WhereIf(input.State.HasValue, x => x.State == input.State)
                  .ApplySearchFilter(input.Keyword, x => x.Name)
                  .AsQueryable();

                var result = query.ApplySort(input.OrderBy, input.SortBy)
                            .ApplySort(OrderByNotification.RECEIVER_GROUP_CODE, SortBy.DESC)
                            .OrderByDescending(x => x.CreationTime)
                            .Skip(input.SkipCount).Take(input.MaxResultCount).ToList();

                var data = DataResult.ResultSuccess(result, "Get success!", query.Count());
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }

        }

        public async Task<object> GetNotificationByIdAsync(long id)
        {
            try
            {
                var data = await _cityNotificationRepos.GetAsync(id);
                var rs = data.MapTo<CityNotificationDto>();
                rs.CountComment = (from com in _commentRepos.GetAll()
                                   where com.CityNotificationId == rs.Id
                                   && !com.IsLike.HasValue
                                   select com).AsQueryable().Count();
                rs.CountFollow = (from com in _commentRepos.GetAll()
                                  where com.CityNotificationId == rs.Id
                                  && com.IsLike == true
                                  select com).AsQueryable().Count();
                return DataResult.ResultSuccess(rs, "Success!");
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> GetAllCommentAsync(GetCommentInput input)
        {
            try
            {
                var query = (from cm in _commentRepos.GetAll()
                             join us in _userRepos.GetAll() on cm.CreatorUserId equals us.Id into tb_us
                             from us in tb_us.DefaultIfEmpty()
                             select new CommentDto()
                             {
                                 CreationTime = cm.CreationTime,
                                 CreatorUserId = cm.CreatorUserId,
                                 Id = cm.Id,
                                 Type = cm.Type,
                                 Comment = cm.Comment,
                                 IsLike = cm.IsLike,
                                 FullName = us != null ? us.UserName : null,
                                 Avatar = us != null ? us.ImageUrl : null,
                                 CityNotificationId = cm.CityNotificationId

                             })
                            .Where(x => !x.IsLike.HasValue)
                            .WhereIf(input.NotificationId > 0, u => u.CityNotificationId == input.NotificationId)
                            .ApplySearchFilter(input.Keyword, x => x.FullName, x => x.Comment)
                            .AsQueryable();

                var result = query
                            .ApplySort(input.OrderBy, input.SortBy)
                            .ApplySort(OrderByComment.FULL_NAME)
                            .PageBy(input).ToList();

                //    var result = await _commentRepos.GetAllListAsync(x => x.CityNotificationId == input.NotifiactionId);
                var data = DataResult.ResultSuccess(result, "Get success!");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }

        }
        public async Task<object> CreatOrUpdateCommentAsync(CommentDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                input.TenantId = AbpSession.TenantId;
                if (input.Id > 0)
                {
                    //update
                    var updateData = await _commentRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);
                        //call back
                        await _commentRepos.UpdateAsync(updateData);
                    }
                    mb.statisticMetris(t1, 0, "Ud_ad_comment");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {

                    var insertInput = input.MapTo<CityNotificationComment>();
                    long id = await _commentRepos.InsertAndGetIdAsync(insertInput);
                    insertInput.Id = id;
                    mb.statisticMetris(t1, 0, "is_ad_comment");
                    var data = DataResult.ResultSuccess(insertInput, "Insert success !");
                    return data;
                }

            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> DeleteCommentAsync(long commentId)
        {
            try
            {
                var comment = await _commentRepos.FirstOrDefaultAsync(x => x.Id == commentId);
                await _commentRepos.DeleteAsync(comment);
                var data = DataResult.ResultSuccess("Delete success!");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> DeleteMultipleCommentsAsync([FromBody] List<long> ids)
        {
            try
            {
                if (ids.Count() == 0) return DataResult.ResultError("Err", "input empty");
                StringBuilder sb = new StringBuilder();

                foreach (long id in ids)
                {
                    sb.AppendFormat("{0},", id);
                }
                var sql = string.Format("UPDATE \"CityNotificationComments\"" +
                    " SET \"IsDeleted\" = true, \"DeleterUserId\" = {1}, \"DeletionTime\" = CURRENT_TIMESTAMP " +
                    " WHERE \"Id\" IN ({0})",
                    sb.ToString().TrimEnd(','),
                    AbpSession.UserId
                );
                var par = new SqlParameter();
                var i = await _sqlExecute.ExecuteAsync(sql);
                CurrentUnitOfWork.SaveChanges();
                var data = DataResult.ResultSuccess("Delete Success");
                //mb.statisticMetris(t1, 0, "admin_del_obj");
                return data;

                //var tasks = new List<Task>();
                //foreach (var id in ids)
                //{
                //    var tk = DeleteNotification(id);
                //    tasks.Add(tk);
                //}
                ////Task.WaitAll(tasks.ToArray());
                //await Task.WhenAll(tasks);

                //var data = DataResult.ResultSuccess("Delete success!");
                //return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> CreatOrUpdateFollowAsync(CommentDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var updateData = await _commentRepos.FirstOrDefaultAsync(x => x.CreatorUserId == AbpSession.UserId && x.IsLike.HasValue && x.Comment == null && x.CityNotificationId == input.CityNotificationId);

                if (updateData != null)
                {
                    //update
                    updateData.IsLike = input.IsLike;
                    await _commentRepos.UpdateAsync(updateData);
                    mb.statisticMetris(t1, 0, "Ud_ad_follow");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {

                    var insertInput = input.MapTo<CityNotificationComment>();
                    insertInput.TenantId = AbpSession.TenantId;
                    insertInput.CreatorUserId = AbpSession.UserId;
                    long id = await _commentRepos.InsertAndGetIdAsync(insertInput);
                    insertInput.Id = id;
                    mb.statisticMetris(t1, 0, "is_ad_follow");
                    var data = DataResult.ResultSuccess(insertInput, "Insert success !");
                    return data;
                }

            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> CreatOrUpdateLikeAsync(LikeDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var updateData = await _userFeedbackLikeRepos.FirstOrDefaultAsync(x => x.CreatorUserId == AbpSession.UserId);

                if (updateData != null)
                {
                    //update
                    await _userFeedbackLikeRepos.UpdateAsync(updateData);
                    mb.statisticMetris(t1, 0, "Ud_ad_like");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {
                    var insertInput = input.MapTo<CitizenReflectLike>();
                    insertInput.TenantId = AbpSession.TenantId;
                    insertInput.CreatorUserId = AbpSession.UserId;
                    long id = await _userFeedbackLikeRepos.InsertAndGetIdAsync(insertInput);
                    insertInput.Id = id;
                    mb.statisticMetris(t1, 0, "is_ad_like");
                    var data = DataResult.ResultSuccess(insertInput, "Insert success !");
                    return data;
                }

            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> GetAllNotificationUser()
        {
            try
            {
                var result = await (from noti in _cityNotificationRepos.GetAll()
                                    join userNoti in _userCityNotificationRepos.GetAll() on AbpSession.UserId equals userNoti.UserId into tb_user_noti
                                    from userNoti in tb_user_noti.DefaultIfEmpty()
                                    where noti.Id == userNoti.CityNotificationId
                                    select noti)
                    .OrderByDescending(m => m.CreationTime)
                    .ToListAsync();
                var data = DataResult.ResultSuccess(result, "Get success!");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }

        }

        public async Task<object> CreateOrUpdateNotification(CityNotificationDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                input.TenantId = AbpSession.TenantId;

                if (input.Id > 0)
                {
                    //update
                    var updateData = await _cityNotificationRepos.GetAsync(input.Id);

                    if (updateData != null)
                    {
                        input.MapTo(updateData);
                        //call back
                        await _cityNotificationRepos.UpdateAsync(updateData);
                    }
                    mb.statisticMetris(t1, 0, "Ud_noti");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {

                    var insertInput = input.MapTo<CityNotification>();
                    var createrName = "Ban quản trị";
                    if (input.OrganizationUnitId.HasValue)
                    {
                        var org = _appOrganizationUnitRepository.FirstOrDefault(x => x.Id == input.OrganizationUnitId);
                        if (org != null) createrName = org.DisplayName;
                    }

                    long id = await _cityNotificationRepos.InsertAndGetIdAsync(insertInput);
                    insertInput.Id = id;
                    await NotifierNewCityNotificationUser(insertInput, createrName);
                    insertInput.ReceiverGroupCode = insertInput.IsReceiveAll == null || insertInput.IsReceiveAll == true ? null : insertInput.ReceiverGroupCode;


                    mb.statisticMetris(t1, 0, "is_noti");
                    var data = DataResult.ResultSuccess(insertInput, "Insert success !");
                    return data;
                }
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> DeleteNotification(long id)
        {
            try
            {
                var noti = await _cityNotificationRepos.FirstOrDefaultAsync(x => x.Id == id);
                if (noti.Type == (int)NotificationEnum.NOTIFICATION_TYPE.TENANT_NOTIFICATION)
                {
                    var UserNotificationsId = (from userNotification in _userCityNotificationRepos.GetAll()
                                               where userNotification.CityNotificationId == id
                                               select userNotification.Id).ToList();

                    for (int index = 0; index < UserNotificationsId.Count; index++)
                    {
                        await _userCityNotificationRepos.DeleteAsync(UserNotificationsId[index]);
                    }
                }
                await _cityNotificationRepos.DeleteAsync(id);
                var data = DataResult.ResultSuccess("Delete success!");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> DeleteMultipleNotification([FromBody] List<long> ids)
        {
            try
            {
                if (ids.Count() == 0) return DataResult.ResultError("Err", "input empty");
                StringBuilder sb = new StringBuilder();

                foreach (long id in ids)
                {
                    sb.AppendFormat("{0},", id);
                }
                var sql = string.Format("UPDATE \"CityNotifications\"" +
                    " SET \"IsDeleted\" = true,  \"DeleterUserId\" =  {1}, \"DeletionTime\" = CURRENT_TIMESTAMP " +
                    " WHERE \"Id\" IN ({0})",
                    sb.ToString().TrimEnd(','),
                    AbpSession.UserId
                    );
                var par = new SqlParameter();
                var i = await _sqlExecute.ExecuteAsync(sql);
                CurrentUnitOfWork.SaveChanges();
                var data = DataResult.ResultSuccess("Delete Success");
                //mb.statisticMetris(t1, 0, "admin_del_obj");
                return data;

                //var tasks = new List<Task>();
                //foreach (var id in ids)
                //{
                //    var tk = DeleteNotification(id);
                //    tasks.Add(tk);
                //}
                ////Task.WaitAll(tasks.ToArray());
                //await Task.WhenAll(tasks);

                //var data = DataResult.ResultSuccess("Delete success!");
                //return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }


        #region Common
        public static IEnumerable<IEnumerable<T>> ChunkList<T>(IEnumerable<T> source, int chunkSize)
        {
            while (source.Any())
            {
                yield return source.Take(chunkSize);
                source = source.Skip(chunkSize);
            }
        }

        private async Task NotifierNewCityNotificationUser(CityNotification data, string creatorName = "Ban quản trị")
        {
            try
            {
                var detailUrlApp = $"yoolife://app/notification/detail?id={data.Id}";
                var detailUrlWA = $"/notices?id={data.Id}";

                var messageDeclined = new UserMessageNotificationDataBase(
                                AppNotificationAction.CityNotificationNew,
                                AppNotificationIcon.CityNotificationNewIcon,
                                TypeAction.Detail,
                                $"{creatorName} đã tạo một thông báo số mới. Nhấn để xem chi tiết !",
                                detailUrlApp,
                                detailUrlWA
                                );
                //tenant
                if (data.ReceiveAll == RECEIVE_TYPE.TENANT_ALL)
                {
                    var citizens = (from cz in _citizenRepos.GetAll()
                                    select new UserIdentifier(cz.TenantId, cz.AccountId.HasValue ? cz.AccountId.Value : 0)).Distinct().ToList();
                    await _appNotifier.SendMessageNotificationInternalAsync(
                        "Yoolife thông báo số !",
                        $"{creatorName} đã tạo một thông báo số mới. Nhấn để xem chi tiết !",
                        detailUrlApp,
                        detailUrlWA,
                        citizens.ToArray(),
                        messageDeclined,
                        AppType.USER
                        );

                }
                //khu do thi
                else if (data.ReceiveAll == RECEIVE_TYPE.URBAN_ALL)
                {
                    var citizens = (from cz in _citizenRepos.GetAll()
                                    where cz.UrbanId == data.UrbanId
                                    select new UserIdentifier(cz.TenantId, cz.AccountId.HasValue ? cz.AccountId.Value : 0)).Distinct().ToList();
                    await _appNotifier.SendMessageNotificationInternalAsync(
                        "Yoolife thông báo số !",
                        $"{creatorName} đã tạo một thông báo số mới. Nhấn để xem chi tiết !",
                        detailUrlApp,
                        detailUrlWA,
                        citizens.ToArray(),
                        messageDeclined,
                        AppType.USER
                        );

                }
                //toa nha
                else if (data.ReceiveAll == RECEIVE_TYPE.BUIDING_ALL)
                {

                    var citizens = new List<List<UserIdentifier>>();

                    foreach (var building in data.OrganizationUnitIds)
                    {
                        var listCitizens = (from cz in _citizenRepos.GetAll()
                                            join ou in _appOrganizationUnitRepository.GetAll() on cz.UrbanId equals ou.Id
                                            where cz.OrganizationUnitId == building
                                            select new UserIdentifier(cz.TenantId, cz.AccountId.HasValue ? cz.AccountId.Value : 0)).Distinct().ToList();
                        citizens.Add(listCitizens);
                    }


                    await _appNotifier.SendMessageNotificationInternalAsync(
                        "Yoolife thông báo số !",
                        $"{creatorName} đã tạo một thông báo số mới. Nhấn để xem chi tiết !",
                        detailUrlApp,
                        detailUrlWA,
                        citizens.SelectMany(x => x).ToArray(),
                        messageDeclined,
                        AppType.USER
                        );
                }
                //can ho
                else
                {
                    var citizensApartment = (from cz in _citizenRepos.GetAll()
                                             where cz.State == STATE_CITIZEN.ACCEPTED && data.ReceiverGroupCode.Contains(cz.ApartmentCode)
                                             select new UserIdentifier(cz.TenantId, cz.AccountId.HasValue ? cz.AccountId.Value : 0)).Distinct().ToList();
                    await _appNotifier.SendMessageNotificationInternalAsync(
                        "Yoolife thông báo số !",
                        $"{creatorName} đã tạo một thông báo số mới. Nhấn để xem chi tiết !",
                        detailUrlApp,
                        detailUrlWA,
                        citizensApartment.ToArray(),
                        messageDeclined,
                        AppType.USER
                        );
                }

            }catch
            {
                return;
            }
        }

        #endregion
    }
}


