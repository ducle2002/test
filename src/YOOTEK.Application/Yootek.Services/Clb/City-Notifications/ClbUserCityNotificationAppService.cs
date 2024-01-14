using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.UI;
using Yootek.Authorization.Users;
using Yootek.Common.DataResult;
using Yootek.Common.Enum;
using Yootek.Yootek.EntityDb.Clb.City_Notification;
using Yootek.Yootek.Services.Yootek.Clb.Dto;
using Yootek.Services;
using Microsoft.EntityFrameworkCore;

namespace Yootek.Yootek.Services.Yootek.Clb.City_Notifications
{
    public interface IClbUserCityNotificationAppService : IApplicationService
    {
        Task<object> GetAllNotificationUserTenant(ClbNotificationInput input);
        Task<object> CreatOrUpdateCommentAsync(ClbCommentDto input);
        Task<object> GetNotificationUserOffline();
        Task<object> GetAllNotificationUser();
        Task<object> CreatOrUpdateFollowAsync(ClbCommentDto input);
        Task<object> GetAllCommentAsync(GetCommentInput input);
        Task<object> GetCityNotificationById(long id);
    }

    [AbpAuthorize]
    public class ClbUserCityNotificationAppService : YootekAppServiceBase, IClbUserCityNotificationAppService
    {
        private readonly IRepository<ClbCityNotification, long> _cityNotificationRepos;
        private readonly IRepository<ClbCityNotificationComment, long> _commentRepos;
        private readonly IRepository<User, long> _userRepos;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public ClbUserCityNotificationAppService(
            IRepository<ClbCityNotification, long> cityNotificationRepos,
            IRepository<ClbCityNotificationComment, long> commentRepos,
            IUnitOfWorkManager unitOfWorkManager, IRepository<User, long> userRepos)
        {
            _cityNotificationRepos = cityNotificationRepos;
            _commentRepos = commentRepos;
            _unitOfWorkManager = unitOfWorkManager;
            _userRepos = userRepos;
        }

        public async Task<object> GetAllCommentAsync(GetCommentInput input)
        {
            try
            {
                var id = AbpSession.UserId;
                var query = (from cm in _commentRepos.GetAll()
                        join us in _userRepos.GetAll() on cm.CreatorUserId equals us.Id into tb_us
                        from us in tb_us.DefaultIfEmpty()
                        where !cm.IsLike.HasValue
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
                    .WhereIf(input.NotificationId > 0, u => u.CityNotificationId == input.NotificationId)
                    .AsQueryable();


                var result = query.PageBy(input).ToList();

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


        public async Task<object> CreatOrUpdateFollowAsync(ClbCommentDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var updateData = await _commentRepos.FirstOrDefaultAsync(x =>
                    x.CreatorUserId == AbpSession.UserId && x.IsLike.HasValue && x.Comment == null &&
                    x.CityNotificationId == input.CityNotificationId);

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

                    mb.statisticMetris(t1, 0, "Ud_comment");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {
                    var insertInput = input.MapTo<ClbCityNotificationComment>();
                    long id = await _commentRepos.InsertAndGetIdAsync(insertInput);
                    insertInput.Id = id;
                    mb.statisticMetris(t1, 0, "is_comment");
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
        
        public async Task<object> GetAllNotificationUserTenant(ClbNotificationInput input)
        {
            try
            {
                var query = (from noti in _cityNotificationRepos.GetAll()
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
                        ReceiverGroupCode = noti.ReceiverGroupCode,
                        IsReceiveAll = noti.IsReceiveAll,
                        DepartmentCode = noti.DepartmentCode,
                        State = noti.State,
                        IsAllowComment = noti.IsAllowComment,
                    })
                    .WhereIf(input.Type == (int)NotificationEnum.NOTIFICATION_TYPE.TENANT_NOTIFICATION, u => u.State == (int)NotificationEnum.NOTIFICATION_STATE.PUSH_NOTIFICATION)
                    .WhereIf(input.Type == (int)NotificationEnum.NOTIFICATION_TYPE.TENANT_DAILYLIFE, u => u.Type == (int)NotificationEnum.NOTIFICATION_TYPE.TENANT_DAILYLIFE)
                    .WhereIf(input.Keyword != null, x => (x.Name != null && x.Name.ToLower().Contains(input.Keyword)))
                    .Where(u => u.DepartmentCode == null || !string.IsNullOrEmpty(u.DepartmentCode))
                    .OrderByDescending(x => x.CreationTime)
                    .AsQueryable();
                var result = query.Skip(input.SkipCount).Take(input.MaxResultCount).ToList();
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

        public async Task<object> GetAllNotificationUser()
        {
            try
            {
                var result = await  _cityNotificationRepos.GetAll()
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

        public async Task<object> GetNotificationUserOffline()
        {
            try
            {
                var result = await _cityNotificationRepos.GetAllListAsync();
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

        public async Task<object> GetCityNotificationById(long id)
        {
            try
            {
                var query = (from noti in _cityNotificationRepos.GetAll()
                    //join cm in _commentRepos.GetAll() on noti.Id equals cm.CityNotificationId into tb_cm
                    //from cm in tb_cm.DefaultIfEmpty()
                    where noti.Id == id
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
                        //  Department = noti.Department,
                        //
                        //OrganizationUnitId = noti.OrganizationUnitId,
                        //  DepartmentCode = noti.DepartmentCode,
                        CountComment = (from com in _commentRepos.GetAll()
                            where com.CityNotificationId == noti.Id
                                  && !com.IsLike.HasValue
                            select com).AsQueryable().Count(),
                        CountFollow = (from com in _commentRepos.GetAll()
                            where com.CityNotificationId == noti.Id
                                  && com.IsLike == true
                            select com).AsQueryable().Count(),
                        ReceiverGroupCode = noti.ReceiverGroupCode,
                        State = noti.State,
                        IsAllowComment = noti.IsAllowComment
                    }).AsQueryable();
                var result = query.FirstOrDefault();
                var count = query.Count();
                var data = DataResult.ResultSuccess(result, "Get success!", count);
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }
    }
}