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
using Yootek.Yootek.EntityDb.Clb.Projects;
using Yootek.Yootek.EntityDb.Forum;
using Yootek.Yootek.Services.Yootek.Clb.Dto;
using Microsoft.EntityFrameworkCore;

namespace Yootek.Services
{
    public interface IProjectAppService : IApplicationService
    {
        Task<object> CreateProjectAsync(CreateProjectDto dto);
        Task<object> UpdateProjectAsync(UpdateProjectDto input);
        Task<object> GetProjectByIdAsync(long id);
        Task<object> GetAllProjectAsync(GetAllProjectDto input);
        Task<object> DeleteProjectAsync(long id);
    }

    [AbpAuthorize]
    public class ProjectAppService : YootekAppServiceBase, IProjectAppService
    {
        private readonly IRepository<Projects, long> _projectRepos;
        private readonly IRepository<ForumPost, long> _postRepos;
        private readonly IRepository<User, long> _userRepos;
        private readonly IRepository<Member, long> _memberRepos;

        public ProjectAppService(
          IRepository<User, long> userRepos,
          IRepository<Projects, long> projectRepos, 
          IRepository<ForumPost, long> postRepos, IRepository<Member, long> memberRepos)
        {
            _userRepos = userRepos;
            _projectRepos = projectRepos;
            _postRepos = postRepos;
            _memberRepos = memberRepos;
        }

        #region Project
        public async Task<object> CreateProjectAsync(CreateProjectDto dto)
        {
            try
            {
                var project = ObjectMapper.Map<Projects>(dto);
                project.TenantId = AbpSession.TenantId;
                project.Status = EProjectStatus.Open;

                await _projectRepos.InsertAsync(project);
                return DataResult.ResultSuccess(true, "Insert success !");
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }
        
        public async Task<object> UpdateProjectAsync(UpdateProjectDto input)
        {
            try
            {
                var Project = await _projectRepos.FirstOrDefaultAsync(x => x.Id == input.Id)
                    ?? throw new UserFriendlyException("Not found !");
                ObjectMapper.Map(input, Project);
                
                await _projectRepos.UpdateAsync(Project);
                return DataResult.ResultSuccess(true, "Update success !");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        
        public async Task<object> GetProjectByIdAsync(long id)
        {
            try
            {
                var prj = await _projectRepos.FirstOrDefaultAsync(x=> x.Id == id);
                
                if(prj == null) throw new UserFriendlyException("Not found !");
                
                var result = ObjectMapper.Map<ProjectDto>(prj);
                result.CountPost = await _postRepos.CountAsync(x => x.Type == (int?)EForumPostType.Project && x.GroupId == prj.Id);
                result.Creator = ObjectMapper.Map<ShortenedUserDto>(await _userRepos.FirstOrDefaultAsync(x => x.Id == prj.CreatorUserId));
                
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

        public async Task<object> GetAllProjectAsync(GetAllProjectDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var query = (from prj in _projectRepos.GetAll()
                        join member in _memberRepos.GetAll() on prj.CreatorUserId equals member.AccountId
                        select new ProjectDto()
                        {
                            Id = prj.Id,
                            TenantId = prj.TenantId,
                            Name = prj.Name,
                            Description = prj.Description,
                            Code = prj.Code,
                            Type = prj.Type,
                            Status = prj.Status,
                            ParentId = prj.ParentId,
                            ManagerId = prj.ManagerId,
                            Capital = prj.Capital,
                            Stage = prj.Stage,
                            StartDate = prj.StartDate,
                            EndDate = prj.EndDate,
                            CountPost = _postRepos.GetAll()
                                .Count(p => p.GroupId == prj.Id && p.Type == (int?)EForumPostType.Project),
                            Creator = new ShortenedUserDto()
                            {
                                Id = member.Id,
                                FullName = member.FullName,
                                ImageUrl = member.ImageUrl,
                                EmailAddress = member.Email,
                            },
                            CreationTime = prj.CreationTime,
                            LastModificationTime = prj.LastModificationTime
                        })
                    .WhereIf(input.Type.HasValue, x => x.Type == input.Type)
                    .WhereIf(input.Status.HasValue, x => x.Status == input.Status)
                    .WhereIf(input.Stage.HasValue, x => x.Stage == input.Stage)
                    .WhereIf(input.StartDate.HasValue, x => x.StartDate == input.StartDate);
                
                var result = await query
                    .PageBy(input)
                    .ApplySort(input.OrderBy, input.SortBy)
                    .ToListAsync();
                
                var data = DataResult.ResultSuccess(result, "Get success!", query.Count());
                mb.statisticMetris(t1, 0, "get_all_project");
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
        
        public async Task<object> DeleteProjectAsync(long id)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var project = await _projectRepos.FirstOrDefaultAsync(x => x.Id == id)
                              ?? throw new UserFriendlyException("Not found !");
                    
                    await _projectRepos.DeleteAsync(project);
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
