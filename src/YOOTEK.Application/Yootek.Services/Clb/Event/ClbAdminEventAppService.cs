using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Yootek.ApbCore.Data;
using Yootek.Application;
using Yootek.Common.DataResult;
using Yootek.Yootek.EntityDb.Clb.Event;
using Yootek.Yootek.EntityDb.Forum;
using Yootek.Yootek.Services.Yootek.Clb.Dto;
using Yootek.Notifications;
using Yootek.Service;
using Yootek.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Yootek.Yootek.Services.Yootek.Clb.Event
{
    public interface IClbAdminEventAppService : IApplicationService
    {
        Task<object> GetAllEvent(ClbEventInput input);
        Task<object> CreateCommentAsync(CreateClbEventCommentDto input);
        Task<object> UpdateCommentAsync(UpdateClbEventCommentDto input);
        Task<object> CreateFollowAsync(CreateClbEventCommentDto input);
        Task<object> UpdateFollowAsync(UpdateClbEventCommentDto input);
        Task<object> GetAllCommentAsync(GetClbEventCommentInput input);
        Task<object> CreateEvent(CreateClbEvent input);
        Task<object> UpdateEvent(UpdateClbEvent input);
        Task<object> DeleteEvent(long id);
        Task<object> DeleteCommentAsync(long commentId);
        Task<object> DeleteMultipleCommentsAsync([FromBody] List<long> ids);
        Task<object> DeleteMultipleEvent([FromBody] List<long> ids);
    }

    [AbpAuthorize]
    public class ClbAdminEventAppService : YootekAppServiceBase, IClbAdminEventAppService
    {
        private readonly IRepository<ClbEvent, long> _eventRepos;
        private readonly IRepository<ClbEventComment, long> _commentRepos;
        private readonly IRepository<ClbUserEvent, long> _userEventRepos; //luu cu dan nhan thong bao
        private readonly IRepository<Member, long> _memberRepos;
        private readonly ISqlExecuter _sqlExecute;
        private readonly IAppNotifier _appNotifier;

        public ClbAdminEventAppService(
            IRepository<ClbEvent, long> eventRepos,
            IRepository<ClbEventComment, long> commentRepos,
            IRepository<ClbUserEvent, long> userEventRepos,
            IAppNotifier appNotifier,
            IRepository<Member, long> memberRepos,
            ISqlExecuter sqlExecute
        )
        {
            _eventRepos = eventRepos;
            _commentRepos = commentRepos;
            _userEventRepos = userEventRepos;
            _sqlExecute = sqlExecute;
            _appNotifier = appNotifier;
            _memberRepos = memberRepos;
        }

        private IQueryable<ClbEventDto> QueryDataEvent(ClbEventInput input)
        {
            var query = (from clbEvent in _eventRepos.GetAll()
                    select new ClbEventDto()
                    {
                        CreationTime = clbEvent.CreationTime,
                        CreatorUserId = clbEvent.CreatorUserId,
                        Description = clbEvent.Description,
                        FileUrl = clbEvent.FileUrl,
                        Id = clbEvent.Id,
                        Name = clbEvent.Name,
                        CountComment = (from com in _commentRepos.GetAll()
                            where com.EventId == clbEvent.Id
                                  && !com.IsLike.HasValue
                            select com).AsQueryable().Count(),
                        CountFollow = (from com in _commentRepos.GetAll()
                            where com.EventId == clbEvent.Id
                                  && com.IsLike == true
                            select com).AsQueryable().Count(),
                        IsAllowComment = clbEvent.IsAllowComment,
                        Creator = _memberRepos.GetAll().Where(x => x.AccountId == clbEvent.CreatorUserId)
                            .Select(x => new MemberShortenedDto()
                            {
                                Id = x.Id,
                                FullName = x.FullName,
                                Email = x.Email,
                                ImageUrl = x.ImageUrl,
                                Type = (int)x.Type
                            }).FirstOrDefault(),
                        Location = clbEvent.Location,
                        Organizer = clbEvent.Organizer,
                        StartTime = clbEvent.StartTime,
                        EndTime = clbEvent.EndTime,
                        AttachUrls = clbEvent.AttachUrls,
                        TenantId = clbEvent.TenantId,
                        IsFollow = (from com in _commentRepos.GetAll()
                            where com.EventId == clbEvent.Id
                                  && com.CreatorUserId == AbpSession.UserId
                                  && com.IsLike == true
                            select com).AsQueryable().Any()
                    }).AsQueryable()
                .WhereIf(input.IsAllowComment.HasValue, u => u.IsAllowComment == input.IsAllowComment)
                .WhereIf(input.FromDay.HasValue, u => u.StartTime >= input.FromDay)
                .WhereIf(input.ToDay.HasValue, u => u.EndTime <= input.ToDay)
                .WhereIf(input.IsFollow.HasValue, u => u.IsFollow == input.IsFollow);


            return query;
        }

        public async Task<object> GetAllEvent(ClbEventInput input)
        {
            try
            {
                List<long> buIds = UserManager.GetAccessibleBuildingOrUrbanIds();
                var query = QueryDataEvent(input)
                    .ApplySearchFilter(input.Keyword, x => x.Name, x => x.Description, x => x.Location,
                        x => x.Organizer).AsQueryable();

                var result = query.ApplySort(input.OrderBy, input.SortBy)
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

        public async Task<object> GetEventByIdAsync(long id)
        {
            try
            {
                var data = await _eventRepos.GetAsync(id);
                var rs = ObjectMapper.Map<ClbEventDto>(data);
                
                rs.CountComment = (from com in _commentRepos.GetAll()
                    where com.EventId == rs.Id
                          && !com.IsLike.HasValue
                    select com).AsQueryable().Count();
                rs.CountFollow = (from com in _commentRepos.GetAll()
                    where com.EventId == rs.Id
                          && com.IsLike == true
                    select com).AsQueryable().Count();
                
                rs.Creator = _memberRepos.GetAll().Where(x => x.AccountId == rs.CreatorUserId)
                    .Select(x => new MemberShortenedDto()
                    {
                        Id = x.Id,
                        FullName = x.FullName,
                        Email = x.Email,
                        ImageUrl = x.ImageUrl,
                        Type = (int)x.Type
                    }).FirstOrDefault();

                return DataResult.ResultSuccess(rs, "Success!");
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> GetAllCommentAsync(GetClbEventCommentInput input)
        {
            try
            {
                var query = (from cm in _commentRepos.GetAll()
                        join us in _memberRepos.GetAll() on cm.CreatorUserId equals us.AccountId into tb_us
                        from us in tb_us.DefaultIfEmpty()
                        select new ClbEventCommentDto()
                        {
                            CreationTime = cm.CreationTime,
                            CreatorUserId = cm.CreatorUserId,
                            Id = cm.Id,
                            Type = cm.Type,
                            Comment = cm.Comment,
                            IsLike = cm.IsLike,
                            EventId = cm.EventId,
                            Creator =  new MemberShortenedDto()
                            {
                                Id = us.Id,
                                FullName = us.FullName,
                                Email = us.Email,
                                ImageUrl = us.ImageUrl,
                                Type = (int)us.Type
                            }
                        })
                    .Where(x => !x.IsLike.HasValue)
                    .WhereIf(input.NotificationId > 0, u => u.EventId == input.NotificationId)
                    .ApplySearchFilter(input.Keyword, x => x.Creator.FullName, x => x.Comment)
                    .AsQueryable();

                var result = query
                    .ApplySort(input.OrderBy, input.SortBy)
                    .ApplySort(OrderByComment.FULL_NAME)
                    .PageBy(input).ToList();

                //    var result = await _commentRepos.GetAllListAsync(x => x.EventId == input.NotifiactionId);
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

        public async Task<object> CreateCommentAsync(CreateClbEventCommentDto input)
        {
            try
            {
                var t1 = TimeUtils.GetNanoseconds();

                input.TenantId = AbpSession.TenantId;
                var insertInput = ObjectMapper.Map<ClbEventComment>(input);
                var id = await _commentRepos.InsertAndGetIdAsync(insertInput);
                insertInput.Id = id;
                
                mb.statisticMetris(t1, 0, "is_ad_comment");
                var data = DataResult.ResultSuccess(insertInput, "Insert success !");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }
        
        public async Task<object> UpdateCommentAsync(UpdateClbEventCommentDto input)
        {
            try
            {
                var t1 = TimeUtils.GetNanoseconds();

                var updateData = await _commentRepos.FirstOrDefaultAsync(x =>
                                     x.CreatorUserId == AbpSession.UserId && !x.IsLike.HasValue &&
                                     x.EventId == input.EventId) ??
                                 throw new UserFriendlyException("Not found !");

                ObjectMapper.Map(input, updateData);
                await _commentRepos.UpdateAsync(updateData);
                mb.statisticMetris(t1, 0, "Ud_ad_comment");

                var data = DataResult.ResultSuccess(updateData, "Update success !");
                return data;
            }
            catch (Exception e)
            {
                DataResult.ResultError(e.ToString(), "Exception !");
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

                var sql = string.Format("UPDATE EventComments" +
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
        
        public async Task<object> CreateFollowAsync(CreateClbEventCommentDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var updateData = await _commentRepos.FirstOrDefaultAsync(x =>
                    x.CreatorUserId == AbpSession.UserId && x.IsLike.HasValue && x.Comment == null &&
                    x.EventId == input.EventId);

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
                    var insertInput = ObjectMapper.Map<ClbEventComment>(input);
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
        
        public async Task<object> UpdateFollowAsync(UpdateClbEventCommentDto input)
        {
            try
            {
                var t1 = TimeUtils.GetNanoseconds();

                var updateData = await _commentRepos.FirstOrDefaultAsync(x =>
                                     x.CreatorUserId == AbpSession.UserId && x.IsLike.HasValue &&
                                     x.EventId == input.EventId) ??
                                 throw new UserFriendlyException("Not found !");

                ObjectMapper.Map(input, updateData);
                
                await _commentRepos.UpdateAsync(updateData);
                mb.statisticMetris(t1, 0, "Ud_ad_follow");

                var data = DataResult.ResultSuccess(updateData, "Update success !");
                return data;
            }
            catch (Exception e)
            {
                DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }

        }
        
        public async Task<object> CreateEvent(CreateClbEvent input)
        {
            try
            {
                var t1 = TimeUtils.GetNanoseconds();

                input.TenantId = AbpSession.TenantId;

                var insertInput = ObjectMapper.Map<ClbEvent>(input);
                
                var createrName = "Quản trị viên";

                var id = await _eventRepos.InsertAndGetIdAsync(insertInput);
                insertInput.Id = id;
                
                await NotifierNewEventUser(insertInput, createrName);
                mb.statisticMetris(t1, 0, "is_noti");
                var data = DataResult.ResultSuccess(insertInput, "Insert success !");
                return data;
            }
            catch (Exception e)
            {
                DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> UpdateEvent(UpdateClbEvent input)
        {
            try
            {   
                var t1 = TimeUtils.GetNanoseconds();

                var updateData = await _eventRepos.GetAsync(input.Id);
                if (updateData != null)
                {
                    var newData = ObjectMapper.Map(input, updateData);
                    
                    await _eventRepos.UpdateAsync(newData);
                    mb.statisticMetris(t1, 0, "Ud_noti");
                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {
                    var data = DataResult.ResultError("Not found !", "Not found !");
                    return data;
                }
            }
            catch (Exception e)
            {
                DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> DeleteEvent(long id)
        {
            try
            {
                var data = await _eventRepos.GetAsync(id)?? throw new UserFriendlyException("Not found !");
                
                var userEvent = await _userEventRepos.GetAll().Where(x => x.EventId == id).ToListAsync();
                if (userEvent.Count > 0)
                {
                    foreach (var item in userEvent)
                    {
                        await _userEventRepos.DeleteAsync(item);
                    }
                }
                
                await _eventRepos.DeleteAsync(data);
                var result = DataResult.ResultSuccess("Delete success !");
                return result;
            }
            catch (Exception e)
            {
                DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> DeleteMultipleEvent([FromBody] List<long> ids)
        {
            try
            {
                if (ids.Count() == 0) return DataResult.ResultError("Err", "input empty");
                StringBuilder sb = new StringBuilder();

                foreach (var id in ids)
                {
                    sb.AppendFormat("{0},", id);
                }

                var sql = string.Format("UPDATE Events" +
                                        " SET IsDeleted = 1,  DeleterUserId =  {1}, DeletionTime = CURRENT_TIMESTAMP " +
                                        " WHERE Id IN ({0})",
                    sb.ToString().TrimEnd(','),
                    AbpSession.UserId
                );
                var par = new SqlParameter();
                var i = await _sqlExecute.ExecuteAsync(sql);
                await CurrentUnitOfWork.SaveChangesAsync();
                var data = DataResult.ResultSuccess("Delete Success");
                //mb.statisticMetris(t1, 0, "admin_del_obj");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        #region Common

        private async Task NotifierNewEventUser(ClbEvent data, string creatorName = "Ban quản trị")
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
            
            var member = (from cz in _memberRepos.GetAll()
                select new UserIdentifier(cz.TenantId, cz.AccountId.HasValue ? cz.AccountId.Value : 0)).Distinct().ToList();
            
            await _appNotifier.SendUserMessageNotifyFireBaseAsync(
                "Yoolife thông báo số !",
                $"{creatorName} đã tạo một thông báo số mới. Nhấn để xem chi tiết !",
                detailUrlApp,
                "",
                member.ToArray(),
                messageDeclined);
        }

        #endregion
    }
}