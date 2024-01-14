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
using Yootek.Yootek.EntityDb.Clb.Event;
using Yootek.Yootek.EntityDb.Forum;
using Yootek.Yootek.Services.Yootek.Clb.Dto;
using Yootek.Service;
using Microsoft.EntityFrameworkCore;

namespace Yootek.Yootek.Services.Yootek.Clb.Event
{
    public interface IClbUserEventAppService : IApplicationService
    {
        Task<object> GetAllEvent(ClbEventInput input);
        Task<object> CreateCommentAsync(CreateClbEventCommentDto input);
        Task<object> UpdateCommentAsync(UpdateClbEventCommentDto input);
        Task<object> CreateFollowAsync(CreateClbEventFollowDto input);
        Task<object> UpdateFollowAsync(UpdateClbEventFollowDto input);
        Task<object> GetAllCommentAsync(GetClbCommentInput input);
        Task<object> GetEventById(long id);
    }

    [AbpAuthorize]
    public class ClbUserEventAppService : YootekAppServiceBase, IClbUserEventAppService
    {
        private readonly IRepository<ClbEvent, long> _eventRepos;
        private readonly IRepository<ClbEventComment, long> _commentRepos;
        private readonly IRepository<Member, long> _memberRepos;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public ClbUserEventAppService(
            IRepository<ClbEvent, long> eventRepos,
            IRepository<ClbEventComment, long> commentRepos,
            IUnitOfWorkManager unitOfWorkManager, 
            IRepository<Member, long> memberRepos)
        {
            _eventRepos = eventRepos;
            _commentRepos = commentRepos;
            _unitOfWorkManager = unitOfWorkManager;
            _memberRepos = memberRepos;
        }

        public async Task<object> GetAllCommentAsync(GetClbCommentInput input)
        {
            try
            {
                var id = AbpSession.UserId;
                var query = (from cm in _commentRepos.GetAll()
                        join us in _memberRepos.GetAll() on cm.CreatorUserId equals us.AccountId into tb_us
                        from us in tb_us.DefaultIfEmpty()
                        where !cm.IsLike.HasValue
                        select new ClbEventCommentDto()
                        {
                            CreationTime = cm.CreationTime,
                            CreatorUserId = cm.CreatorUserId,
                            Id = cm.Id,
                            Type = cm.Type,
                            Comment = cm.Comment,
                            IsLike = cm.IsLike,
                            EventId = cm.EventId,
                            Creator = new MemberShortenedDto()
                            {
                                Id = us.Id,
                                FullName = us.FullName,
                                ImageUrl = us.ImageUrl,
                                Email = us.Email,
                                Type = (int) us.Type
                            }
                        })
                    .WhereIf(input.NotificationId > 0, u => u.EventId == input.NotificationId)
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
        
        public async Task<object> CreateFollowAsync(CreateClbEventFollowDto input)
        {
            try
            {
                var t1 = TimeUtils.GetNanoseconds();

                input.TenantId = AbpSession.TenantId;
                var exist = await _commentRepos.FirstOrDefaultAsync(x => x.EventId == input.EventId && x.CreatorUserId == AbpSession.UserId);
                if (exist != null)
                {
                    exist.IsLike = true;
                    await _commentRepos.UpdateAsync(exist);
                    mb.statisticMetris(t1, 0, "Ud_comment");
                    var res = DataResult.ResultSuccess(exist, "Update success !");
                    return res;
                }

                var insertInput = ObjectMapper.Map<ClbEventComment>(input);

                var id = await _commentRepos.InsertAndGetIdAsync(insertInput);
                insertInput.Id = id;
                
                mb.statisticMetris(t1, 0, "is_comment");
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
        
        public async Task<object> UpdateFollowAsync(UpdateClbEventFollowDto input)
        {
            try
            {
                var t1 = TimeUtils.GetNanoseconds();

                input.TenantId = AbpSession.TenantId;
                var updateData = await _commentRepos.GetAsync(input.Id) ?? throw new UserFriendlyException("Not found !");
                
                ObjectMapper.Map(input, updateData);
                await _commentRepos.UpdateAsync(updateData);

                mb.statisticMetris(t1, 0, "Ud_comment");

                var data = DataResult.ResultSuccess(updateData, "Update success !");
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
                
                mb.statisticMetris(t1, 0, "is_comment");
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

                input.TenantId = AbpSession.TenantId;
                var updateData = await _commentRepos.GetAsync(input.Id) ?? throw new UserFriendlyException("Not found !");
                
                ObjectMapper.Map(input, updateData);
                await _commentRepos.UpdateAsync(updateData);

                mb.statisticMetris(t1, 0, "Ud_comment");

                var data = DataResult.ResultSuccess(updateData, "Update success !");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }

        }
        
        public async Task<object> GetAllEvent(ClbEventInput input)
        {
            try
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
                        Creator = (from us in _memberRepos.GetAll()
                            where us.Id == clbEvent.CreatorUserId
                            select new MemberShortenedDto()
                            {
                                Id = us.Id,
                                FullName = us.FullName,
                                ImageUrl = us.ImageUrl,
                                Email = us.Email,
                                Type = (int) us.Type
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
                    })
                    .WhereIf(input.Keyword != null, x => (x.Name != null && x.Name.ToLower().Contains(input.Keyword)))
                    .WhereIf(input.IsAllowComment.HasValue, u => u.IsAllowComment == input.IsAllowComment)
                    .WhereIf(input.FromDay.HasValue, u => u.StartTime >= input.FromDay)
                    .WhereIf(input.ToDay.HasValue, u => u.EndTime <= input.ToDay)
                    .WhereIf(input.IsFollow.HasValue, u => u.IsFollow == input.IsFollow)
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

        public async Task<object> GetAllEventUser()
        {
            try
            {
                var result = await  _eventRepos.GetAll()
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

        public async Task<object> GetEventById(long id)
        {
            try
            {
                var query = (from noti in _eventRepos.GetAll()
                    where noti.Id == id
                    select new ClbEventDto()
                    {
                        CreationTime = noti.CreationTime,
                        CreatorUserId = noti.CreatorUserId,
                        Description = noti.Description,
                        FileUrl = noti.FileUrl,
                        Id = noti.Id,
                        Name = noti.Name,
                        CountComment = (from com in _commentRepos.GetAll()
                            where com.EventId == noti.Id
                                  && !com.IsLike.HasValue
                            select com).AsQueryable().Count(),
                        CountFollow = (from com in _commentRepos.GetAll()
                            where com.EventId == noti.Id
                                  && com.IsLike == true
                            select com).AsQueryable().Count(),
                        IsAllowComment = noti.IsAllowComment,
                        Creator = (from us in _memberRepos.GetAll()
                            where us.Id == noti.CreatorUserId
                            select new MemberShortenedDto()
                            {
                                Id = us.Id,
                                FullName = us.FullName,
                                ImageUrl = us.ImageUrl,
                                Email = us.Email,
                                Type = (int) us.Type
                            }).FirstOrDefault(),
                        Location = noti.Location,
                        Organizer = noti.Organizer,
                        StartTime = noti.StartTime,
                        EndTime = noti.EndTime,
                        AttachUrls = noti.AttachUrls,
                        TenantId = noti.TenantId,
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