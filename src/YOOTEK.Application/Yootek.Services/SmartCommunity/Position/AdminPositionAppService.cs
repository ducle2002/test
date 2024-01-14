using Abp.Application.Services;
using Abp.Domain.Repositories;
using Yootek.Common.DataResult;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yootek.EntityDb;
using Abp.AutoMapper;
using Yootek.Organizations;
using Abp.Authorization.Users;
using Abp.Collections.Extensions;
using Microsoft.AspNetCore.Mvc;
using Abp.Linq.Extensions;
using Microsoft.EntityFrameworkCore;
using Abp.UI;

namespace Yootek.Services
{
    //[AbpAuthorize]
    public class AdminPositionAppService : YootekAppServiceBase
    {
        private readonly IRepository<Position, long> _positionRepos;
        private readonly AppOrganizationUnitManager _organizationUnitManager;
        private readonly IRepository<CityVote, long> _cityVoteRepos;
        private readonly IRepository<AppOrganizationUnit, long> _appOrganizationUnitRepository;
        private readonly IRepository<UserOrganizationUnit, long> _userOrganizationUnitRepository;

        public AdminPositionAppService(
            IRepository<Position, long> positionRepos,
            IRepository<CityVote, long> cityVoteRepos,
            IRepository<AppOrganizationUnit, long> appOrganizationUnitRepository,
            IRepository<UserOrganizationUnit, long> userOrganizationUnitRepository

            )
        {
            _positionRepos = positionRepos;
            _cityVoteRepos = cityVoteRepos;
            _appOrganizationUnitRepository = appOrganizationUnitRepository;
            _userOrganizationUnitRepository = userOrganizationUnitRepository;
        }

        public async Task<object> CreateOrUpdatePosition(PositionDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                input.TenantId = AbpSession.TenantId;
                if (input.Id > 0)
                {
                    //update
                    var updateData = await _positionRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);
                        //call back
                        await _positionRepos.UpdateAsync(updateData);
                    }
                    mb.statisticMetris(t1, 0, "Ud_staff");
                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {
                    var insertInput = input.MapTo<Position>();
                    if (insertInput.Code == null)
                    {
                        insertInput.Code = GetUniqueCitizenCode();
                    }
                    else insertInput.Code = input.Code;

                    long id = await _positionRepos.InsertAndGetIdAsync(insertInput);
                    insertInput.Id = id;
                    insertInput.TenantId = AbpSession.TenantId;
                    mb.statisticMetris(t1, 0, "is_staff");
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

        public async Task<object> GetAllPositionAsync(GetPositionInput input)
        {

            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var query = (from po in _positionRepos.GetAll()
                                 select new PositionDto()
                                 {
                                     CreationTime = po.CreationTime,
                                     CreatorUserId = po.CreatorUserId,
                                     Id = po.Id,
                                     Name = po.Name,
                                     OrganizationUnitId = po.OrganizationUnitId,
                                     Description = po.Description,
                                     DisplayName = po.DisplayName,
                                     Code = po.Code,
                                     TenantId = po.TenantId,
                                     BuildingId = (from ou in _appOrganizationUnitRepository.GetAll()
                                                   where po.OrganizationUnitId == ou.Id
                                                   select ou.ParentId).FirstOrDefault(),

                                 })
                                 .WhereIf(input.OrganizationUnitId != null, x => input.OrganizationUnitId == x.OrganizationUnitId)
                                 .WhereIf(input.BuildingId != null, x => input.BuildingId == x.BuildingId)
                                 .WhereIf(input.Keyword != null, x => (x.Name != null && x.Name.ToLower().Contains(input.Keyword.ToLower())))
                                 .AsQueryable();

                    var result = await query.PageBy(input).ToListAsync();

                    var data = DataResult.ResultSuccess(result, "Get success!", query.Count());
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

        public async Task<object> GetPositionById(long id)
        {
            try
            {
                var data = (from po in _positionRepos.GetAll()
                             select new PositionDto()
                             {
                                 CreationTime = po.CreationTime,
                                 CreatorUserId = po.CreatorUserId,
                                 Id = po.Id,
                                 Name = po.Name,
                                 OrganizationUnitId = po.OrganizationUnitId,
                                 Description = po.Description,
                                 DisplayName = po.DisplayName,
                                 Code = po.Code,
                                 TenantId = po.TenantId,
                                 BuildingId = (from ou in _appOrganizationUnitRepository.GetAll()
                                               where po.OrganizationUnitId == ou.Id
                                               select ou.ParentId).FirstOrDefault(),

                             }).Select(x => x.Id == id).FirstOrDefault();
                return DataResult.ResultSuccess(data, "Success!");
            } catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<DataResult> DeletePosition(long id)
        {
            try
            {

                await _positionRepos.DeleteAsync(id);
                var data = DataResult.ResultSuccess("Delete success!");
                return data;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public Task<DataResult> DeleteListPosition([FromBody] List<long> ids)
        {
            try
            {

                if (ids.Count == 0) return Task.FromResult(DataResult.ResultError("Err", "input empty"));
                var tasks = new List<Task>();
                foreach (var id in ids)
                {
                    var tk = DeletePosition(id);
                    tasks.Add(tk);
                }
                Task.WaitAll(tasks.ToArray());

                var data = DataResult.ResultSuccess("Delete success!");
                return Task.FromResult(data);
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                return Task.FromResult(data);
            }
        }
    }
}
