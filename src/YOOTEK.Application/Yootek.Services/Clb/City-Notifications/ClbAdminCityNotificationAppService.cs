using Abp;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
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
using Yootek.Yootek.EntityDb.Clb.City_Notification;
using Yootek.Yootek.Services.Yootek.Clb.Dto;
using Yootek.QueriesExtension;

namespace Yootek.Services
{
    public interface IClbAdminCityNotificationAppService : IApplicationService
    {
        Task<object> GetAllNotificationUserTenant(ClbNotificationInput input);

        Task<object> GetAllNotificationUser();
        Task<object> CreatOrUpdateCommentAsync(ClbCommentDto input);
        Task<object> CreatOrUpdateFollowAsync(ClbCommentDto input);
        Task<object> GetAllCommentAsync(GetClbCommentInput input);
        Task<object> CreateOrUpdateNotification(ClbCityNotificationDto input);
        Task<object> DeleteNotification(long id);
        Task<object> DeleteCommentAsync(long commentId);

    }

    [AbpAuthorize]
    public class ClbAdminCityNotificationAppService : YootekAppServiceBase, IClbAdminCityNotificationAppService
    {
        private readonly IRepository<ClbCityNotification, long> _cityNotificationRepos;
        private readonly IRepository<ClbCityNotificationComment, long> _commentRepos;
        private readonly IRepository<User, long> _userRepos;
        private readonly IRepository<ClbUserCityNotification, long> _userCityNotificationRepos; //luu cu dan nhan thong bao
        private readonly IRepository<Citizen, long> _citizenRepos;
        private readonly IRepository<Apartment, long> _smartHomeRepos;
        private readonly ISqlExecuter _sqlExecute;
        private readonly IAppNotifier _appNotifier;

        public ClbAdminCityNotificationAppService(
            IRepository<ClbCityNotification, long> cityNotificationRepos,
            IRepository<ClbCityNotificationComment, long> commentRepos,
            IRepository<User, long> userRepos,
            IRepository<ClbUserCityNotification, long> userCityNotificationRepos,
            IAppNotifier appNotifier,
            IRepository<Citizen, long> citizenRepos,
            IRepository<Apartment, long> smartHomeRepos,
            ISqlExecuter sqlExecute
            )
        {
            _cityNotificationRepos = cityNotificationRepos;
            _commentRepos = commentRepos;
            _userCityNotificationRepos = userCityNotificationRepos;
            _userRepos = userRepos;
            _sqlExecute = sqlExecute;
            _appNotifier = appNotifier;
            _citizenRepos = citizenRepos;
            _smartHomeRepos = smartHomeRepos;
        }

        protected IQueryable<ClbCityNotificationDto> QueryDataCityNotification(ClbNotificationInput input)
        {

            var query = (from noti in _cityNotificationRepos.GetAll()
                where noti.Type == input.Type
                         select new ClbCityNotificationDto()
                         {
                             CreationTime = noti.CreationTime,
                             CreatorUserId = noti.CreatorUserId,
                             Data = noti.Data,
                             FileUrl = noti.FileUrl,
                             Follow = noti.Follow,
                             Id = noti.Id,
                             Name = noti.Name,
                             Type = noti.Type,
                             CountComment = (from com in _commentRepos.GetAll()
                                             where com.CityNotificationId == noti.Id
                                             && !com.IsLike.HasValue
                                             select com).AsQueryable().Count(),
                             CountFollow = (from com in _commentRepos.GetAll()
                                            where com.CityNotificationId == noti.Id
                                            && com.IsLike == true
                                            select com).AsQueryable().Count(),
                             ReceiverGroupCode = noti.IsReceiveAll == null || noti.IsReceiveAll == true ? null : noti.ReceiverGroupCode,
                             State = noti.State,
                             IsAllowComment = noti.IsAllowComment,
                             ReceiveAll = noti.ReceiveAll,
                         }).AsQueryable();

            return query;
        }

        public async Task<object> GetAllNotificationUserTenant(ClbNotificationInput input)
        {
            try
            {
                List<long> buIds = UserManager.GetAccessibleBuildingOrUrbanIds();
                var query = QueryDataCityNotification(input)
                  .WhereByBuildingOrUrbanIf(!IsGranted(IOCPermissionNames.Data_Admin), buIds)
                  .WhereIf(input.ReceiverGroupCode != null, x => input.ReceiverGroupCode == x.ReceiverGroupCode)
                    .WhereIf(input.ReceiveAll != null, x => input.ReceiveAll == x.ReceiveAll)
                    .WhereIf(input.State.HasValue, x => x.State == input.State)
                    .ApplySearchFilter(input.Keyword, x => x.Name).AsQueryable();

                var result = query.ApplySort(input.OrderBy, input.SortBy)
                            .ApplySort(OrderByNotification.RECEIVER_GROUP_CODE, SortBy.DESC)
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
                var rs = data.MapTo<ClbCityNotificationDto>();
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

        public async Task<object> GetAllCommentAsync(GetClbCommentInput input)
        {
            try
            {
                var query = (from cm in _commentRepos.GetAll()
                             join us in _userRepos.GetAll() on cm.CreatorUserId equals us.Id into tb_us
                             from us in tb_us.DefaultIfEmpty()
                             select new ClbCommentDto()
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
        public async Task<object> CreatOrUpdateCommentAsync(ClbCommentDto input)
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

                    var insertInput = input.MapTo<ClbCityNotificationComment>();
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
                var sql = string.Format("UPDATE CityNotificationComments" +
                                        " SET IsDeleted = 1,  DeleterUserId =  {1}, DeletionTime = CURRENT_TIMESTAMP " +
                                        " WHERE Id IN ({0})",
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

        public async Task<object> CreatOrUpdateFollowAsync(ClbCommentDto input)
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

                    var insertInput = input.MapTo<ClbCityNotificationComment>();
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

        public async Task<object> CreateOrUpdateNotification(ClbCityNotificationDto input)
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

                    var insertInput = input.MapTo<ClbCityNotification>();
                    var createrName = "Ban quản trị";

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
                var sql = string.Format("UPDATE CityNotifications" +
                    " SET IsDeleted = 1,  DeleterUserId =  {1}, DeletionTime = CURRENT_TIMESTAMP " +
                    " WHERE Id IN ({0})",
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

        private async Task NotifierNewCityNotificationUser(ClbCityNotification data, string creatorName = "Ban quản trị")
        {
            var detailUrlApp = $"yoolife://app/notification/detail?id={data.Id}";
            var messageDeclined = new NotificationWithContentIdDatabase(
                            data.Id,
                            AppNotificationAction.CityNotificationNew,
                            AppNotificationIcon.CityNotificationNewIcon,
                            TypeAction.Detail,
                            $"{creatorName} đã tạo một thông báo số mới. Nhấn để xem chi tiết !",
                            "",
                            "",
                            detailUrlApp
                            );
            //tenant
            if (data.ReceiveAll == RECEIVE_TYPE.TENANT_ALL)
            {
                var citizens = (from cz in _citizenRepos.GetAll()
                                select new UserIdentifier(cz.TenantId, cz.AccountId.HasValue ? cz.AccountId.Value : 0)).Distinct().ToList();
                await _appNotifier.SendUserMessageNotifyFireBaseAsync(
                     "Yoolife thông báo số !",
                     $"{creatorName} đã tạo một thông báo số mới. Nhấn để xem chi tiết !",
                     detailUrlApp,
                     "",
                     citizens.ToArray(),
                   messageDeclined);
            }
            //khu do thi
            else if (data.ReceiveAll == RECEIVE_TYPE.URBAN_ALL)
            {
                var citizens = (from cz in _citizenRepos.GetAll()
                                select new UserIdentifier(cz.TenantId, cz.AccountId.HasValue ? cz.AccountId.Value : 0)).Distinct().ToList();
                await _appNotifier.SendUserMessageNotifyFireBaseAsync(
                     "Yoolife thông báo số !",
                     $"{creatorName} đã tạo một thông báo số mới. Nhấn để xem chi tiết !",
                     detailUrlApp,
                     "",
                     citizens.ToArray(),
                   messageDeclined);
            }
            //toa nha
            else if (data.ReceiveAll == RECEIVE_TYPE.BUIDING_ALL)
            {
                var citizens = (from cz in _citizenRepos.GetAll()
                    select new UserIdentifier(cz.TenantId, cz.AccountId.HasValue ? cz.AccountId.Value : 0)).Distinct().ToList();
                await _appNotifier.SendUserMessageNotifyFireBaseAsync(
                     "Yoolife thông báo số !",
                     $"{creatorName} đã tạo một thông báo số mới. Nhấn để xem chi tiết !",
                     detailUrlApp,
                     "",
                     citizens.ToArray(),
                   messageDeclined);
            }
            //can ho
            else
            {
                var citizensApartment = (from cz in _citizenRepos.GetAll()
                                         join sh in _smartHomeRepos.GetAll()
                                         on cz.ApartmentCode equals sh.ApartmentCode
                                         where cz.State == STATE_CITIZEN.ACCEPTED && data.ReceiverGroupCode.Contains(cz.ApartmentCode)
                                         select new UserIdentifier(cz.TenantId, cz.AccountId.HasValue ? cz.AccountId.Value : 0)).Distinct().ToList();
                await _appNotifier.SendUserMessageNotifyFireBaseAsync(
                     "Yoolife thông báo số !",
                     $"{creatorName} đã tạo một thông báo số mới. Nhấn để xem chi tiết !",
                     detailUrlApp,
                     "",
                     citizensApartment.ToArray(),
                   messageDeclined);
            }
        }

        #endregion
    }
}


