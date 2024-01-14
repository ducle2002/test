using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using Yootek.Authorization.Users;
using Yootek.Common.DataResult;
using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Linq.Extensions;
using Yootek.App.ServiceHttpClient.Dto.Social.Forum;
using Yootek.Application;
using Yootek.EntityDb;
using Yootek.Yootek.EntityDb.Clb.Jobs;
using Yootek.Yootek.EntityDb.Forum;
using Yootek.Yootek.Services.Yootek.Clb.Dto;
using Microsoft.EntityFrameworkCore;

namespace Yootek.Services
{
    public interface IJobAppService : IApplicationService
    {
        Task<object> CreateJobAsync(CreateJobDto dto);
        Task<object> UpdateJobAsync(UpdateJobDto input);
        Task<object> GetJobByIdAsync(long id);
        Task<object> GetAllJobAsync(GetAllJobDto input);
        Task<object> DeleteJobAsync(long id);
    }

    [AbpAuthorize]
    public class JobAppService : YootekAppServiceBase, IJobAppService
    {
        private readonly IRepository<Jobs, long> _jobRepos;
        private readonly IRepository<ForumPost, long> _postRepos;
        private readonly IRepository<Member, long> _memberRepos;
        private readonly IRepository<User, long> _userRepos;

        public JobAppService(
            IRepository<User, long> userRepos,
            IRepository<Jobs, long> jobRepos, IRepository<ForumPost, long> postRepos, IRepository<Member, long> memberRepos)
        {
            _userRepos = userRepos;
            _jobRepos = jobRepos;
            _postRepos = postRepos;
            _memberRepos = memberRepos;
        }

        #region Citizen

        public async Task<object> CreateJobAsync(CreateJobDto dto)
        {
            try
            {
                var job = ObjectMapper.Map<Jobs>(dto);
                job.TenantId = AbpSession.TenantId;
                job.Status = EJobsStatus.Open;

                await _jobRepos.InsertAsync(job);
                return DataResult.ResultSuccess(true, "Insert success !");
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> UpdateJobAsync(UpdateJobDto input)
        {
            try
            {
                var job = await _jobRepos.FirstOrDefaultAsync(x => x.Id == input.Id)
                          ?? throw new UserFriendlyException("Not found !");
                ObjectMapper.Map(input, job);

                await _jobRepos.UpdateAsync(job);
                return DataResult.ResultSuccess(true, "Update success !");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> GetJobByIdAsync(long id)
        {
            try
            {
                var query = (from job in _jobRepos.GetAll()
                        join member in _memberRepos.GetAll() on job.CreatorUserId equals member.AccountId
                        select new JobDto()
                        {
                            Id = job.Id,
                            TenantId = job.TenantId,
                            Name = job.Name,
                            Description = job.Description,
                            Salary = job.Salary,
                            Position = job.Position,
                            Requirement = job.Requirement,
                            Benefit = job.Benefit,
                            EnterpriseName = job.EnterpriseName,
                            EnterpriseAddress = job.EnterpriseAddress,
                            EnterprisePhone = job.EnterprisePhone,
                            EnterpriseEmail = job.EnterpriseEmail,
                            Type = job.Type,
                            Status = job.Status,
                            CountPost = _postRepos.GetAll()
                                .Count(p => p.GroupId == job.Id && p.Type == (int?)EForumPostType.Job),
                            Creator = new ShortenedUserDto()
                            {
                                Id = member.Id,
                                FullName = member.FullName,
                                ImageUrl = member.ImageUrl,
                                EmailAddress = member.Email,
                            }
                        }
                    )
                    .Where(x => x.Id == id);
                
                var result = await query.FirstOrDefaultAsync();
                
                if (result == null) throw new UserFriendlyException("Not found !");

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

        public async Task<object> GetAllJobAsync(GetAllJobDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var query = (from job in _jobRepos.GetAll()
                        join member in _memberRepos.GetAll() on job.CreatorUserId equals member.AccountId
                        select new JobDto()
                        {
                            Id = job.Id,
                            TenantId = job.TenantId,
                            Name = job.Name,
                            Description = job.Description,
                            Salary = job.Salary,
                            Position = job.Position,
                            Requirement = job.Requirement,
                            Benefit = job.Benefit,
                            EnterpriseName = job.EnterpriseName,
                            EnterpriseAddress = job.EnterpriseAddress,
                            EnterprisePhone = job.EnterprisePhone,
                            EnterpriseEmail = job.EnterpriseEmail,
                            Type = job.Type,
                            Status = job.Status,
                            CountPost = _postRepos.GetAll()
                                .Count(p => p.GroupId == job.Id && p.Type == (int?)EForumPostType.Job),
                            Creator = new ShortenedUserDto()
                            {
                                Id = member.Id,
                                FullName = member.FullName,
                                ImageUrl = member.ImageUrl,
                                EmailAddress = member.Email,
                            },
                            CreationTime = job.CreationTime,
                            LastModificationTime = job.LastModificationTime
                        }
                    )
                    .ApplySearchFilter(input.Keyword, x => x.Name, x => x.Description, x => x.Position);
                    
                    query = query.WhereIf(input.Salary.HasValue, x => x.Salary >= input.Salary)
                    .WhereIf(input.Type.HasValue, x => x.Type == input.Type)
                    .WhereIf(input.Status.HasValue, x => x.Status == input.Status);
                
                var result = await query
                    .PageBy(input)
                    .ApplySort(input.OrderBy, input.SortBy)
                    .ToListAsync();
                var data = DataResult.ResultSuccess(result, "Get success!", query.Count());
                mb.statisticMetris(t1, 0, "get_info_citizen");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        #endregion

        public async Task<object> DeleteJobAsync(long id)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var job = await _jobRepos.FirstOrDefaultAsync(x => x.Id == id)
                              ?? throw new UserFriendlyException("Not found !");

                    await _jobRepos.DeleteAsync(job);
                    return DataResult.ResultSuccess(true, "Delete success !");
                }
            }

            catch (Exception e)
            {
                DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }
    }
}