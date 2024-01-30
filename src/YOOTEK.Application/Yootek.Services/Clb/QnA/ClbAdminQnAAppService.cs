//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Abp.Application.Services;
//using Abp.AutoMapper;
//using Abp.Domain.Repositories;
//using Abp.Linq.Extensions;
//using Abp.UI;
//using Yootek.Application;
//using Yootek.Authorization.Users;
//using Yootek.Common.DataResult;
//using Yootek.Common.Enum;
//using Yootek.Yootek.EntityDb.Clb.QnA;
//using Yootek.Services.Dto;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using static Yootek.Common.Enum.CommonENum;

//namespace Yootek.Yootek.Services.Yootek.Clb.QnA
//{
//    public interface IClbAdminQnAAppService : IApplicationService { }
//    public class ClbAdminQnAAppService : YootekAppServiceBase, IClbAdminQnAAppService
//    {
//        private readonly IRepository<ClbForum, long> _forumRepos;
//        private readonly IRepository<ClbForumComment, long> _forumCommentRepos;
//        private readonly IRepository<User, long> _userRepos;
//        private readonly IRepository<ClbForumTopic, long> _topicRepos;
//        private readonly IClbAdminQuestionAnswerExcelExporter _adminQuestionAnswerExcelExporter;
//        public ClbAdminQnAAppService(
//            IRepository<ClbForum, long> forumRepos,
//            IRepository<ClbForumComment, long> forumCommentRepos,
//            IRepository<User, long> userRepos,
//            IRepository<ClbForumTopic, long> topicRepos,
//            IClbAdminQuestionAnswerExcelExporter adminQuestionAnswerExcelExporter
//            )
//        {
//            _forumCommentRepos = forumCommentRepos;
//            _forumRepos = forumRepos;
//            _userRepos = userRepos;
//            _topicRepos = topicRepos;
//            _adminQuestionAnswerExcelExporter = adminQuestionAnswerExcelExporter;
//        }
//        protected IQueryable<ClbQuestionAnswerDto> QueryDataQNA(GetAllClbQuestionAnswerInput input)
//        {
//            DateTime fromDay = new DateTime(), toDay = new DateTime();
//            if (input.FromDay.HasValue)
//            {
//                fromDay = new DateTime(input.FromDay.Value.Year, input.FromDay.Value.Month, input.FromDay.Value.Day, 0, 0, 0);

//            }
//            if (input.ToDay.HasValue)
//            {
//                toDay = new DateTime(input.ToDay.Value.Year, input.ToDay.Value.Month, input.ToDay.Value.Day, 23, 59, 59);

//            }

//            var query = (from fr in _forumRepos.GetAll()
//                         join us in _userRepos.GetAll() on fr.CreatorUserId equals us.Id into tb_us
//                         from us in tb_us.DefaultIfEmpty()
//                         select new ClbQuestionAnswerDto
//                         {
//                             Id = fr.Id,
//                             FileUrl = fr.FileUrl,
//                             Type = fr.Type,
//                             Content = fr.Content,
//                             State = fr.State,
//                             CreationTime = fr.CreationTime,
//                             CreatorAvatar = us.ImageUrl,
//                             CreatorName = us.FullName,
//                             CreatorUserId = fr.CreatorUserId,
//                             LastModificationTime = fr.LastModificationTime,
//                             LastModifierUserId = fr.LastModifierUserId,
//                             Tags = fr.Tags,
//                             TenantId = fr.TenantId,
//                             ThreadTitle = fr.ThreadTitle,
//                             TypeTitle = fr.TypeTitle,
//                             CommentCount = (from cm in _forumCommentRepos.GetAll()
//                                             where cm.ForumId == fr.Id
//                                             select cm).Count(),
//                             IsAdminAnswered = (from cm in _forumCommentRepos.GetAll() where (cm.ForumId == fr.Id && cm.IsAdmin.Value) select cm).Any(),
//                             TopicId = fr.TopicId,
//                         })
//                         //.WhereIf(input.Type.HasValue, u => u.Type == input.Type)
//                         .ApplySearchFilter(input.Keyword, x => x.ThreadTitle)
//                         .WhereIf(input.Id.HasValue, x => x.Id == input.Id.Value)
//                         .WhereIf(input.IsAdminAnswered.HasValue, x => x.IsAdminAnswered == input.IsAdminAnswered)
//                         .WhereIf(input.FromDay.HasValue && !input.ToDay.HasValue, x => x.CreationTime.Date == fromDay.Date)
//                         .WhereIf(input.FromDay.HasValue && input.ToDay.HasValue, x => x.CreationTime.Date >= fromDay.Date && x.CreationTime.Date <= toDay.Date)
//                         .AsQueryable();
//            #region Truy van tung Form
//            switch (input.FormId.Value)
//            {
//                case (int)CommonENumForum.FORM_ID_FORUM.ADMIN_GETALL_ACCEPT:
//                    query = query.Where(x => x.State == (int)CommonENumForum.FORUM_STATE.ACCEPT);
//                    break;
//                case (int)CommonENumForum.FORM_ID_FORUM.ADMIN_GETALL_NEW:
//                    query = query.Where(x => x.State == (int)CommonENumForum.FORUM_STATE.NEW);
//                    break;
//                case (int)CommonENumForum.FORM_ID_FORUM.ADMIN_GETALL_DISABLE:
//                    query = query.Where(x => x.State == (int)CommonENumForum.FORUM_STATE.DISABLE);
//                    break;
//                case (int)CommonENumForum.FORM_ID_FORUM.ADMIN_GETALL:
//                    break;
//                case (int)CommonENumForum.FORM_ID_FORUM.USER_GETALL:
//                    query = query.Where(x => x.State == (int)CommonENumForum.FORUM_STATE.ACCEPT);
//                    break;
//                case (int)CommonENumForum.FORM_ID_FORUM.CREATOR_GETALL:
//                    query = query.Where(x => x.CreatorUserId == AbpSession.UserId);
//                    break;
//                default:
//                    query = query.Take(0);
//                    break;
//            }
//            #endregion
//            return query;
//        }
//        public async Task<object> GetAllQuestionAnswerSocialAsync(GetAllClbQuestionAnswerInput input)
//        {
//            try
//            {
//                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
//                {
//                    long t1 = TimeUtils.GetNanoseconds();
//                    var query = QueryDataQNA(input);
//                    var list = await query
//                        .ApplySort(input.OrderBy, input.SortBy)
//                        .ApplySort(OrderByQuestionAnswer.CREATION_TIME, SortBy.DESC)
//                        .PageBy(input).ToListAsync();

//                    var data = DataResult.ResultSuccess(list, "Get success", query.Count());
//                    mb.statisticMetris(t1, 0, "gall_forum");
//                    return data;
//                }
//            }
//            catch (Exception ex)
//            {
//                var data = DataResult.ResultError(ex.ToString(), "Exception");
//                Logger.Fatal(ex.Message, ex);
//                throw;
//            }
//        }

//        public async Task<object> GetByIdAsync(long id)
//        {
//            try
//            {
//                var data = await _forumRepos.GetAsync(id);
//                var data1 = data.MapTo<ClbQuestionAnswerDto>();
//                data1.CommentCount = (from cm in _forumCommentRepos.GetAll()
//                                      where cm.ForumId == data1.Id
//                                      select cm).Count();
//                return DataResult.ResultSuccess(data1, "Success!");
//            }
//            catch (Exception ex)
//            {
//                var data = DataResult.ResultError(ex.ToString(), "Exception");
//                Logger.Fatal(ex.Message, ex);
//                throw;
//            }
//        }
//        public async Task<object> GetAllCommentByQuestionAnswerAsync(GetAllClbCommentForumSocialInput input)
//        {
//            try
//            {

//                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
//                {
//                    long t1 = TimeUtils.GetNanoseconds();

//                    var query = (from cm in _forumCommentRepos.GetAll()
//                                 where cm.ForumId == input.ForumId
//                                 join us in _userRepos.GetAll() on cm.CreatorUserId equals us.Id into tb_us
//                                 from us in tb_us.DefaultIfEmpty()
//                                 select new ClbCommentForumDto
//                                 {
//                                     FileUrl = cm.FileUrl,
//                                     CreatorName = us.FullName,
//                                     CreatorAvatar = us.ImageUrl,
//                                     CreationTime = cm.CreationTime,
//                                     Comment = cm.Comment,
//                                     TenantId = cm.TenantId,
//                                     Id = cm.Id,
//                                     ForumId = cm.ForumId,
//                                     CreatorUserId = cm.CreatorUserId,
//                                     IsAdmin = cm.IsAdmin,
//                                 })
//                       .AsQueryable();
//                    var list = await query.PageBy(input).ToListAsync();

//                    var data = DataResult.ResultSuccess(list, "Get success", query.Count());
//                    mb.statisticMetris(t1, 0, "gall_commentforum");
//                    return data;
//                }
//            }
//            catch (Exception ex)
//            {
//                var data = DataResult.ResultError(ex.ToString(), "Exception");
//                Logger.Fatal(ex.Message, ex);
//                throw;
//            }
//        }

//        public async Task<object> GetQuestionAnswerById(GetAllClbQuestionAnswerInput input)
//        {
//            try
//            {
//                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
//                {
//                    long t1 = TimeUtils.GetNanoseconds();
//                    var result = QueryDataQNA(input).FirstOrDefaultAsync().Result;

//                    var data = DataResult.ResultSuccess(result, "Get success");
//                    mb.statisticMetris(t1, 0, "gall_forum");
//                    return data;
//                }
//            }
//            catch (Exception ex)
//            {
//                var data = DataResult.ResultError(ex.ToString(), "Exception");
//                Logger.Fatal(ex.Message, ex);
//                throw;
//            }
//        }

//        [Obsolete]
//        public async Task<object> CreateOrUpdateCommentQuestionAndAnswer(ClbCommentForumInputDto input)
//        {
//            try
//            {
//                long t1 = TimeUtils.GetNanoseconds();
//                input.TenantId = AbpSession.TenantId;
//                if (input.Id > 0)
//                {
//                    //update
//                    var updateData = await _forumCommentRepos.GetAsync(input.Id);
//                    if (updateData != null)
//                    {
//                        input.MapTo(updateData);
//                        updateData.IsAdmin = true;
//                        //call back
//                        await _forumCommentRepos.UpdateAsync(updateData);
//                    }
//                    mb.statisticMetris(t1, 0, "update_forum");

//                    var data = DataResult.ResultSuccess(updateData, "Update success !");
//                    return data;
//                }
//                else
//                {
//                    //Insert
//                    var insertInput = input.MapTo<ClbForumComment>();
//                    insertInput.IsAdmin = true;
//                    long id = await _forumCommentRepos.InsertAndGetIdAsync(insertInput);

//                    mb.statisticMetris(t1, 0, "insert_forum");
//                    var data = DataResult.ResultSuccess(insertInput, "Insert success !");
//                    return data;
//                }
//            }
//            catch (Exception e)
//            {
//                throw;
//            }
//        }

//        public async Task<object> GetAllQuestionAnswerTopics(GetAllClbForumTopicInput input)
//        {
//            try
//            {
//                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
//                {
//                    long t1 = TimeUtils.GetNanoseconds();
//                    var query = _topicRepos.GetAll();
//                    var count = query.Count();
//                    var list = await query.PageBy(input).ToListAsync();

//                    if (list.Count == 0)
//                    {
//                        list = new List<ClbForumTopic>()
//                        {
//                            new ClbForumTopic{Name= "Chủ đề 1", Description="Mô tả 1", Id = 1},
//                            new ClbForumTopic{Name = "Chủ đề 2", Description = "Mô tả 2", Id = 2},
//                            new ClbForumTopic{Name = "Chủ đề 3", Description = "Mô tả 3", Id = 3},
//                            new ClbForumTopic
//                            {
//                                Name = "Chủ đề 4",
//                                Description = "Mô tả 4",
//                                Id = 4,
//                            }
//                        };
//                    }

//                    var data = DataResult.ResultSuccess(list, "Get success", count);
//                    mb.statisticMetris(t1, 0, "gall_topics");
//                    return data;
//                }
//            }
//            catch (Exception e)
//            {
//                throw;
//            }
//        }

//        public async Task<object> GetTopicByIdAsync(long id)
//        {
//            try
//            {
//                var data = await _topicRepos.GetAsync(id);
//                return DataResult.ResultSuccess(data, "Success");
//            }
//            catch (Exception e)
//            {
//                throw;
//            }
//        }

//        public async Task<object> UpdateStateQuestionTopic(long id, int state)
//        {
//            try
//            {
//                long t1 = TimeUtils.GetNanoseconds();
//                var updateData = await _forumRepos.GetAsync(id);
//                if (updateData != null)
//                {
//                    updateData.State = (int)state;
//                    await _forumRepos.UpdateAsync(updateData);
//                }
//                mb.statisticMetris(t1, 0, "update_forum");

//                var data = DataResult.ResultSuccess(updateData, "Update success !");
//                return data;
//            }
//            catch (Exception error)
//            {
//                throw new UserFriendlyException(error.Message);
//            }
//        }

//        [Obsolete]
//        public async Task<object> CreateOrUpdateTopicAsync(ClbForumTopicInputDto input)
//        {
//            try
//            {
//                long t1 = TimeUtils.GetNanoseconds();
//                input.TenantId = AbpSession.TenantId;
//                if (input.Id > 0)
//                {
//                    var updateTopic = await _topicRepos.GetAsync(input.Id);
//                    if (updateTopic != null)
//                    {
//                        input.MapTo(updateTopic);
//                        await _topicRepos.UpdateAsync(updateTopic);
//                    }
//                    mb.statisticMetris(t1, 0, "update_topic");
//                    var result = DataResult.ResultSuccess(updateTopic, "Update success!");
//                    return result;
//                }
//                else
//                {
//                    var insertTopic = input.MapTo<ClbForumTopic>();
//                    long id = await _topicRepos.InsertAndGetIdAsync(insertTopic);
//                    mb.statisticMetris(t1, 0, "insert_topic");
//                    var result = DataResult.ResultSuccess(insertTopic, "Insert success!");
//                    return result;
//                }
//            }
//            catch (Exception e)
//            {
//                return new UserFriendlyException(e.Message);
//            }
//        }

//        public async Task<object> DeleteQuestionAnswerTopicAsync(long id)
//        {
//            try
//            {
//                long t1 = TimeUtils.GetNanoseconds();
//                await _topicRepos.DeleteAsync(id);
//                var data = DataResult.ResultSuccess("Delete Success");
//                mb.statisticMetris(t1, 0, "del_topic");
//                return data;
//            }
//            catch (Exception e)
//            {
//                return new UserFriendlyException(e.Message);
//            }
//        }

//        public Task<DataResult> DeleteMultipleQuestionAnswerTopic([FromBody] List<long> ids)
//        {
//            try
//            {

//                if (ids.Count == 0) return Task.FromResult(DataResult.ResultError("Err", "input empty"));
//                var tasks = new List<Task>();
//                foreach (var id in ids)
//                {
//                    var tk = DeleteQuestionAnswerTopicAsync(id);
//                    tasks.Add(tk);
//                }
//                Task.WaitAll(tasks.ToArray());

//                var data = DataResult.ResultSuccess("Delete success!");
//                return Task.FromResult(data);
//            }
//            catch (Exception e)
//            {
//                var data = DataResult.ResultError(e.ToString(), "Exception !");
//                Logger.Fatal(e.Message);
//                return Task.FromResult(data);
//            }
//        }

//        public async Task<object> ExportAdminForumToExcel(ClbQuestionAnswerExcelOutputDto input)
//        {
//            try
//            {
//                DateTime fromDay = new DateTime(), toDay = new DateTime();
//                if (input.FromDay.HasValue)
//                {
//                    fromDay = new DateTime(input.FromDay.Value.Year, input.FromDay.Value.Month, input.FromDay.Value.Day, 0, 0, 0);

//                }
//                if (input.ToDay.HasValue)
//                {
//                    toDay = new DateTime(input.ToDay.Value.Year, input.ToDay.Value.Month, input.ToDay.Value.Day, 23, 59, 59);

//                }

//                var query = (from fr in _forumRepos.GetAll()
//                             join us in _userRepos.GetAll() on fr.CreatorUserId equals us.Id into tb_us
//                             from us in tb_us.DefaultIfEmpty()
//                             select new ClbExportQuestionAnswerDto
//                             {
//                                 Id = fr.Id,
//                                 FileUrl = fr.FileUrl,
//                                 Type = fr.Type,
//                                 Content = fr.Content,
//                                 State = fr.State,
//                                 CreationTime = fr.CreationTime,
//                                 CreatorAvatar = us.ImageUrl,
//                                 CreatorName = us.FullName,
//                                 CreatorUserId = fr.CreatorUserId,
//                                 LastModificationTime = fr.LastModificationTime,
//                                 LastModifierUserId = fr.LastModifierUserId,
//                                 Tags = fr.Tags,
//                                 TenantId = fr.TenantId,
//                                 ThreadTitle = fr.ThreadTitle,
//                                 TypeTitle = fr.TypeTitle,
//                                 CommentCount = (from cm in _forumCommentRepos.GetAll()
//                                                 where cm.ForumId == fr.Id
//                                                 select cm).Count(),
//                                 IsAdminAnswered = ((from cm in _forumCommentRepos.GetAll() where (cm.ForumId == fr.Id && cm.IsAdmin.Value) select cm).Count()) > 0,
//                                 TopicId = fr.TopicId,
//                                 Comments = (from cm in _forumCommentRepos.GetAll()
//                                             where cm.ForumId == fr.Id
//                                             join user in _userRepos.GetAll() on cm.CreatorUserId equals user.Id into tb_us
//                                             from user in tb_us.DefaultIfEmpty()
//                                             select new ClbCommentForumDto
//                                             {
//                                                 FileUrl = cm.FileUrl,
//                                                 CreatorName = user.FullName,
//                                                 CreatorAvatar = user.ImageUrl,
//                                                 CreationTime = cm.CreationTime,
//                                                 Comment = cm.Comment,
//                                                 TenantId = cm.TenantId,
//                                                 Id = cm.Id,
//                                                 ForumId = cm.ForumId,
//                                                 CreatorUserId = cm.CreatorUserId,
//                                                 IsAdmin = cm.IsAdmin,
//                                             }).ToList()
//                             })
//                             //.WhereIf(input.Type.HasValue, u => u.Type == input.Type)
//                             .WhereIf(input.Keyword != null, x => (x.Content != null && x.Content.ToLower().Contains(input.Keyword)
//                                                    //|| x.CreatorName != null && x.CreatorName.ToLower().Contains(input.Keyword)
//                                                    || x.ThreadTitle != null && x.ThreadTitle.ToLower().Contains(input.Keyword)
//                                                    ))
//                             .WhereIf(input.Id.HasValue, x => x.Id == input.Id.Value)
//                             .WhereIf(input.IsAdminAnswered.HasValue, x => x.IsAdminAnswered == input.IsAdminAnswered)
//                             .WhereIf(input.FromDay.HasValue && !input.ToDay.HasValue, x => x.CreationTime.Date == fromDay.Date)
//                             .WhereIf(input.FromDay.HasValue && input.ToDay.HasValue, x => x.CreationTime.Date >= fromDay.Date && x.CreationTime.Date <= toDay.Date)
//                             .AsQueryable();

//                switch (input.FormId.Value)
//                {
//                    case (int)CommonENumForum.FORM_ID_FORUM.ADMIN_GETALL_ACCEPT:
//                        query = query.Where(x => x.State == (int)CommonENumForum.FORUM_STATE.ACCEPT);
//                        break;
//                    case (int)CommonENumForum.FORM_ID_FORUM.ADMIN_GETALL_NEW:
//                        query = query.Where(x => x.State == (int)CommonENumForum.FORUM_STATE.NEW);
//                        break;
//                    case (int)CommonENumForum.FORM_ID_FORUM.ADMIN_GETALL_DISABLE:
//                        query = query.Where(x => x.State == (int)CommonENumForum.FORUM_STATE.DISABLE);
//                        break;
//                    case (int)CommonENumForum.FORM_ID_FORUM.ADMIN_GETALL:
//                        break;
//                    case (int)CommonENumForum.FORM_ID_FORUM.USER_GETALL:
//                        query = query.Where(x => x.State == (int)CommonENumForum.FORUM_STATE.ACCEPT);
//                        break;
//                    case (int)CommonENumForum.FORM_ID_FORUM.CREATOR_GETALL:
//                        query = query.Where(x => x.CreatorUserId == AbpSession.UserId);
//                        break;
//                    default:
//                        query = query.Take(0);
//                        break;
//                }

//                query = query.OrderByDescending(x => x.CreationTime);

//                var list = await query.WhereIf(input.Ids != null && input.Ids.Count > 0, x => input.Ids.Contains(x.Id))
//                    .ToListAsync();
//                var result = _adminQuestionAnswerExcelExporter.ExportToFile(list);
//                return DataResult.ResultSuccess(result, "Export success");
//            }
//            catch (Exception e)
//            {
//                var data = DataResult.ResultError(e.ToString(), "Exception !");
//                Logger.Fatal(e.Message);
//                throw;
//            }
//        }
//    }
//}
