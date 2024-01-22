using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Yootek.Application;
using Yootek.Authorization.Users;
using Yootek.Common.DataResult;
using Yootek.Yootek.EntityDb.Forum;
using Yootek.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Yootek.Services
{
    public interface IMemberAppService : IApplicationService
    {
        Task<object> GetAllMemberAsync(GetAllMemberInput input);
        Task<object> GetMemberByIdAsync(long id);
        Task<object> CreateMember(CreateMemberByAdminDto input);
        Task<object> UpdateMember(UpdateMemberByAdminDto input);
        Task<DataResult> DeleteMember(long id);
        Task<DataResult> UpdateStateMember(UpdateStateMemberByAdminDto input);
        Task<DataResult> DeleteMultipleMember([FromBody] List<long> ids);
    }

    [AbpAuthorize]
    public class AdminMemberAppService : YootekAppServiceBase, IMemberAppService
    {
        private readonly IRepository<Member, long> _memberRepos;
        private readonly IRepository<User, long> _userRepos;

        public AdminMemberAppService(
            IRepository<User, long> userRepos,
            IRepository<Member, long> memberRepos
        )
        {
            _userRepos = userRepos;
            _memberRepos = memberRepos;
        }

        public async Task<object> CreateMember(CreateMemberByAdminDto input)
        {
            try
            {
                var t1 = TimeUtils.GetNanoseconds();
                var insertInput = ObjectMapper.Map<Member>(input);
                insertInput.TenantId = AbpSession.TenantId;
                await _memberRepos.InsertAsync(insertInput);

                mb.statisticMetris(t1, 0, "create member");
                return DataResult.ResultSuccess(true, "Insert success !");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> UpdateMember(UpdateMemberByAdminDto input)
        {
            try
            {
                input.TenantId = AbpSession.TenantId;
                var updateData = await _memberRepos.GetAsync(input.Id);

                if (updateData == null)
                {
                    return DataResult.ResultFail("Member not found!");
                }

                ObjectMapper.Map(input, updateData);
                await _memberRepos.UpdateAsync(updateData);

                return DataResult.ResultSuccess(true, "Update success !");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public Task<DataResult> DeleteMember(long id)
        {
            try
            {
                _memberRepos.DeleteAsync(id);
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

        private IQueryable<Member> QueryMember(GetAllMemberInput input)
        {
            var query = (from mem in _memberRepos.GetAll()
                    join us in _userRepos.GetAll() on mem.AccountId equals us.Id into tb_us
                    from us in tb_us.DefaultIfEmpty()
                    select new ClbMemberDto()
                    {
                        Id = mem.Id,
                        PhoneNumber = mem.PhoneNumber != null ? mem.PhoneNumber : us.PhoneNumber,
                        Nationality = mem.Nationality,
                        FullName = mem.FullName,
                        IdentityNumber = mem.IdentityNumber,
                        ImageUrl = mem.ImageUrl != null ? mem.ImageUrl : us.ImageUrl,
                        Email = mem.Email != null
                            ? mem.Email
                            : (us.EmailAddress.Contains("yootek") ? us.EmailAddress : null),
                        Address = mem.Address,
                        DateOfBirth = mem.DateOfBirth,
                        AccountId = mem.AccountId,
                        Gender = mem.Gender,
                        State = mem.State,
                        Type = mem.Type,
                        TenantId = mem.TenantId,
                        Career = mem.Career,
                        IdentityImageUrls = mem.IdentityImageUrls
                    })
                .ApplySearchFilter(input.Keyword, x => x.FullName, x => x.Address, x => x.Email)
                .WhereIf(input.State.HasValue, x => x.State == input.State);

            DateTime fromDay, toDay;

            if (input.FromDay.HasValue)
            {
                fromDay = new DateTime(input.FromDay.Value.Year, input.FromDay.Value.Month,
                    input.FromDay.Value.Day, 0, 0, 0);
                query = query.WhereIf(input.FromDay.HasValue,
                    u => (u.LastModificationTime.HasValue && u.LastModificationTime >= fromDay) ||
                         (!u.LastModificationTime.HasValue && u.CreationTime >= fromDay));
            }

            if (input.ToDay.HasValue)
            {
                toDay = new DateTime(input.ToDay.Value.Year, input.ToDay.Value.Month, input.ToDay.Value.Day, 23,
                    59, 59);
                query = query.WhereIf(input.ToDay.HasValue,
                    u => (u.LastModificationTime.HasValue && u.LastModificationTime <= toDay) ||
                         (!u.LastModificationTime.HasValue && u.CreationTime <= toDay));
            }

            return query;
        }

        public async Task<object> GetAllMemberAsync(GetAllMemberInput input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var query = QueryMember(input);

                    var result = await query
                        .ApplySort(input.OrderBy, input.SortBy)
                        .PageBy(input)
                        .ToListAsync();

                    var data = DataResult.ResultSuccess(result, "Get success!", query.Count());
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

        public async Task<object> GetMemberCountStatistic()
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var count = await _memberRepos.GetAll().CountAsync();
                    return DataResult.ResultSuccess(count, "Get success");
                }
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> GetMemberByIdAsync(long id)
        {
            try
            {
                var result = await _memberRepos.GetAsync(id);
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

        public async Task<DataResult> UpdateStateMember(UpdateStateMemberByAdminDto input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var t1 = TimeUtils.GetNanoseconds();
                    //update
                    var updateData = await _memberRepos.GetAsync(input.Id);
                    if (updateData == null) return DataResult.ResultFail("Member not found!");
                    if (updateData.State == input.State) return DataResult.ResultFail("State no change!");
                    updateData.State = input.State;

                    await _memberRepos.UpdateAsync(updateData);
                    await CurrentUnitOfWork.SaveChangesAsync();
                    return DataResult.ResultSuccess(updateData, "Update state success !");
                }
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public Task<DataResult> DeleteMultipleMember([FromBody] List<long> ids)
        {
            try
            {
                if (ids.Count == 0) return Task.FromResult(DataResult.ResultError("Err", "input empty"));

                Task.WaitAll(ids.Select(DeleteMember).Cast<Task>().ToArray());

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