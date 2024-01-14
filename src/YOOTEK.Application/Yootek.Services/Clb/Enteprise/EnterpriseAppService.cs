using Abp.Application.Services;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.UI;
using Yootek.Common.DataResult;
using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Linq.Extensions;
using Yootek.Application;
using Yootek.EntityDb;
using Yootek.Yootek.EntityDb.Clb.Enterprise;
using Yootek.Yootek.EntityDb.Forum;
using Yootek.Yootek.Services.Yootek.Clb.Dto;
using Microsoft.EntityFrameworkCore;

namespace Yootek.Services
{
    public interface IEnterpriseAppService : IApplicationService
    {
        Task<object> GetAllEnterpriseAsync(GetEnterpriseDto input);
        Task<object> GetAllBusinessFieldAsync(GetBusinessFieldDto input);
        Task<object> GetEnterpriseByIdAsync(long id);
        Task<object> GetAllEmployeeOfEnterpriseAsync(GetAllEmployeeOfEnterpriseDto input);
        Task<object> CreateEnterpriseAsync(CreateEnterpriseDto dto);
        Task<object> CreateBusinessFieldAsync(CreateBusinessFieldDto dto);
        Task<object> AddMemberToEnterpriseAsync(AddMemberToEnterpriseDto dto);
        Task<object> LeaveEnterpriseAsync(LeaveEnterpriseDto dto);
        Task<object> UpdateEnterpriseAsync(UpdateEnterpriseDto input);
        Task<object> UpdateBusinessFieldAsync(UpdateBusinessFieldDto input);
        Task<object> UpdateEmployeeAsync(UpdateEmployeeDto input);
        Task<object> DeleteEnterpriseAsync(long id);
        Task<object> DeleteBusinessFieldAsync(long id);
        Task<object> DeleteEmployeeAsync(long id);
    }

    [AbpAuthorize]
    public class EnterpriseAppService : YootekAppServiceBase, IEnterpriseAppService
    {
        private readonly IRepository<Enterprises, long> _enterpriseRepos;
        private readonly IRepository<UserEnterprises, long> _userEnterpriseRepos;
        private readonly IRepository<ForumPost, long> _postRepos;
        private readonly IRepository<BusinessField, long> _businessFieldRepos;
        private readonly IRepository<Member, long> _memberRepos;

        public EnterpriseAppService(
            IRepository<Member, long> memberRepos,
            IRepository<Enterprises, long> enterpriseRepos, IRepository<UserEnterprises, long> userEnterpriseRepos, IRepository<BusinessField, long> businessFieldRepos, IRepository<ForumPost, long> postRepos)
        {
            _memberRepos = memberRepos;
            _enterpriseRepos = enterpriseRepos;
            _userEnterpriseRepos = userEnterpriseRepos;
            _businessFieldRepos = businessFieldRepos;
            _postRepos = postRepos;
        }

        public async Task<object> GetAllEnterpriseAsync(GetEnterpriseDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var query = QueryEnterprise(input);

                    var result = await query
                        .ApplySort(input.OrderBy, input.SortBy)
                        .PageBy(input)
                        .ToListAsync();
                    
                    mb.statisticMetris(t1, 0, "get_all_enterprise");
                    var data = DataResult.ResultSuccess(result, "Get success!", query.Count());
                    return data;
            }

            catch (Exception e)
            {
                DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }
        
        public async Task<object> GetAllBusinessFieldAsync(GetBusinessFieldDto input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var query = (from buf in _businessFieldRepos.GetAll()
                        select new BusinessFieldDto()
                        {
                            Id = buf.Id,
                            Name = buf.Name,
                            Description = buf.Description,
                            EnterpriseCount = _enterpriseRepos.GetAll().Count(x => x.BusinessField == buf.Id)
                        })
                        .ApplySearchFilter(input.Keyword, x => x.Name, x => x.Description);

                    var result = await query.Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();

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

        public async Task<object> GetEnterpriseByIdAsync(long id)
        {
            try
            {
                var result = await _enterpriseRepos.GetAll().Select(
                        ent=> new EnterpriseDto()
                        {
                            Id = ent.Id,
                            Name = ent.Name,
                            ImageUrl = ent.ImageUrl,
                            FoundedDate = ent.FoundedDate,
                            Email = ent.Email,
                            MemberCount = _userEnterpriseRepos.GetAll().Count(x => x.EnterpriseId == ent.Id),
                            ProjectCount = null,
                            PostCount = _postRepos.GetAll().Count(x => x.GroupId == ent.Id && x.Type == (int?)EForumPostType.Business),
                            Phone = ent.Phone,
                            Status = ent.Status,
                            Type = ent.Type,
                            Website = ent.Website,
                            TaxCode = ent.TaxCode,
                            OwnerName = ent.OwnerName,
                            BusinessField = ent.BusinessField,
                            WardId = ent.WardId,
                            ProvinceId = ent.ProvinceId,
                            DistrictId = ent.DistrictId,
                            Address = ent.Address,
                            Description = ent.Description,
                            TenantId = ent.TenantId,
                            LastModificationTime = ent.LastModificationTime,
                            CreationTime = ent.CreationTime
                        }
                    ).FirstOrDefaultAsync();
                if (result == null) throw new UserFriendlyException("Doanh nghiệp không tồn tại");

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
        
        public async Task<object> GetAllEmployeeOfEnterpriseAsync(GetAllEmployeeOfEnterpriseDto input)
        {
            try
            {
                var query = (from ue in _userEnterpriseRepos.GetAll()
                        join user in _memberRepos.GetAll() on ue.MemberId equals user.Id
                        where ue.EnterpriseId == input.EnterpriseId
                        select new EmployeeDto()
                        {
                            Id = ue.Id,
                            TenantId = ue.TenantId,
                            MemberId = ue.MemberId,
                            EnterpriseId = ue.EnterpriseId,
                            FullName = user.FullName,
                            Email = user.Email,
                            PhoneNumber = user.PhoneNumber,
                            Role = ue.Role,
                            Status = ue.Status,
                            Description = ue.Description
                        })
                    .ApplySearchFilter(input.Keyword, x => x.FullName, x => x.Email, x => x.PhoneNumber);

                var result = await query
                    .ApplySort(input.OrderBy, input.SortBy)
                    .PageBy(input)
                    .ToListAsync();

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

        public async Task<object> CreateEnterpriseAsync(CreateEnterpriseDto dto)
        {
            try
            {
                var member = ObjectMapper.Map<Enterprises>(dto);
                member.TenantId = AbpSession.TenantId;
                member.Status = EnterpriseStatus.Active;

                await _enterpriseRepos.InsertAsync(member);
                return DataResult.ResultSuccess(true, "Insert success !");
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }
        
        public async Task<object> CreateBusinessFieldAsync(CreateBusinessFieldDto dto)
        {
            try
            {
                var member = ObjectMapper.Map<BusinessField>(dto);
                member.TenantId = AbpSession.TenantId;

                await _businessFieldRepos.InsertAsync(member);
                return DataResult.ResultSuccess(true, "Insert success !");
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }
        
        public async Task<object> AddMemberToEnterpriseAsync(AddMemberToEnterpriseDto dto)
        {
            try
            {
                var member = await _memberRepos.FirstOrDefaultAsync(x => x.Id == dto.MemberId);
                if (member == null) throw new UserFriendlyException("Thành viên không tồn tại");
                var enterprise = await _enterpriseRepos.FirstOrDefaultAsync(x => x.Id == dto.EnterpriseId);
                if (enterprise == null) throw new UserFriendlyException("Doanh nghiệp không tồn tại");
                var userEnterprise = await _userEnterpriseRepos.FirstOrDefaultAsync(x =>
                    x.MemberId == dto.MemberId && x.EnterpriseId == dto.EnterpriseId);
                if (userEnterprise != null) throw new UserFriendlyException("Thành viên đã tồn tại trong doanh nghiệp");
                var userEnterpriseNew = new UserEnterprises()
                {
                    TenantId = AbpSession.TenantId,
                    MemberId = dto.MemberId,
                    EnterpriseId = dto.EnterpriseId,
                    Role = dto.Role ?? UserEnterpriseRole.Employee,
                    Status = UserEnterpriseStatus.Active,
                    Description = dto.Description,
                };
                
                await _userEnterpriseRepos.InsertAsync(userEnterpriseNew);
                return DataResult.ResultSuccess(true, "Insert success !");
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }
        
        public async Task<object> LeaveEnterpriseAsync(LeaveEnterpriseDto dto)
        {
            try
            {
                var userEnterprise = (from ue in _userEnterpriseRepos.GetAll()
                    join user in _memberRepos.GetAll() on ue.MemberId equals user.Id
                    where ue.EnterpriseId == dto.EnterpriseId && user.AccountId == AbpSession.UserId
                    select ue).FirstOrDefault();
                if (userEnterprise == null) throw new UserFriendlyException("Thành viên không tồn tại trong doanh nghiệp");
                
                await _userEnterpriseRepos.DeleteAsync(userEnterprise);
                return DataResult.ResultSuccess(true, "Delete success !");
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> UpdateEnterpriseAsync(UpdateEnterpriseDto input)
        {
            try
            {
                var enterprise = await _enterpriseRepos.FirstOrDefaultAsync(x => x.Id == input.Id);
                if (enterprise == null) throw new UserFriendlyException("Doanh nghiệp không tồn tại");
                ObjectMapper.Map(input, enterprise);
                await _enterpriseRepos.UpdateAsync(enterprise);
                return DataResult.ResultSuccess(true, "Update success !");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        
        public async Task<object> UpdateBusinessFieldAsync(UpdateBusinessFieldDto input)
        {
            try
            {
                var businessField = await _businessFieldRepos.FirstOrDefaultAsync(x => x.Id == input.Id);
                if (businessField == null) throw new UserFriendlyException("Lĩnh vực kinh doanh không tồn tại");
                ObjectMapper.Map(input, businessField);
                await _businessFieldRepos.UpdateAsync(businessField);
                return DataResult.ResultSuccess(true, "Update success !");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        
        public async Task<object> UpdateEmployeeAsync(UpdateEmployeeDto input)
        {
            try
            {
                var employee = await _userEnterpriseRepos.FirstOrDefaultAsync(x => x.Id == input.Id);
                if (employee == null) throw new UserFriendlyException("Nhân viên không tồn tại");
                ObjectMapper.Map(input, employee);
                await _userEnterpriseRepos.UpdateAsync(employee);
                return DataResult.ResultSuccess(true, "Update success !");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        
        public async Task<object> DeleteEnterpriseAsync(long id)
        {
            try
            {
                var enterprise = await _enterpriseRepos.FirstOrDefaultAsync(x => x.Id == id);
                if (enterprise == null) throw new UserFriendlyException("Doanh nghiệp không tồn tại");
                await _enterpriseRepos.DeleteAsync(enterprise);
                return DataResult.ResultSuccess(true, "Delete success !");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        
        public async Task<object> DeleteBusinessFieldAsync(long id)
        {
            try
            {
                var enterprise = await _businessFieldRepos.FirstOrDefaultAsync(x => x.Id == id);
                if (enterprise == null) throw new UserFriendlyException("Lĩnh vực kinh doanh không tồn tại");
                await _businessFieldRepos.DeleteAsync(enterprise);
                return DataResult.ResultSuccess(true, "Delete success !");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        
        public async Task<object> DeleteEmployeeAsync(long id)
        {
            try
            {
                var enterprise = await _userEnterpriseRepos.FirstOrDefaultAsync(x => x.Id == id);
                if (enterprise == null) throw new UserFriendlyException("Nhân viên không tồn tại");
                await _userEnterpriseRepos.DeleteAsync(enterprise);
                return DataResult.ResultSuccess(true, "Delete success !");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        private IQueryable<EnterpriseDto> QueryEnterprise(GetEnterpriseDto input)
        {
            var query = (from ent in _enterpriseRepos.GetAll()
                    select new EnterpriseDto()
                    {
                        Id = ent.Id,
                        Name = ent.Name,
                        ImageUrl = ent.ImageUrl,
                        FoundedDate = ent.FoundedDate,
                        Email = ent.Email,
                        MemberCount = _userEnterpriseRepos.GetAll().Count(x => x.EnterpriseId == ent.Id),
                        ProjectCount = null,
                        Phone = ent.Phone,
                        Status = ent.Status,
                        Type = ent.Type,
                        Website = ent.Website,
                        TaxCode = ent.TaxCode,
                        OwnerName = ent.OwnerName,
                        BusinessField = ent.BusinessField,
                        WardId = ent.WardId,
                        ProvinceId = ent.ProvinceId,
                        DistrictId = ent.DistrictId,
                        Address = ent.Address,
                        Description = ent.Description,
                        TenantId = ent.TenantId
                    })
                .ApplySearchFilter(input.Keyword, x => x.Name, x => x.Email)
                .WhereIf(AbpSession.TenantId.HasValue, x => x.TenantId == AbpSession.TenantId);
            
            query = query.WhereIf(input.Type.HasValue, x => x.Type == input.Type);
            query = query.WhereIf(input.Status.HasValue, x => x.Status == input.Status);

            DateTime fromDay, toDay;

            if (input.FromDay.HasValue)
            {
                fromDay = new DateTime(input.FromDay.Value.Year, input.FromDay.Value.Month,
                    input.FromDay.Value.Day, 0, 0, 0);
                query = query.WhereIf(input.FromDay.HasValue,
                    u => u.FoundedDate.HasValue && u.FoundedDate >= fromDay);
            }

            if (input.ToDay.HasValue)
            {
                toDay = new DateTime(input.ToDay.Value.Year, input.ToDay.Value.Month, input.ToDay.Value.Day, 23,
                    59, 59);
                query = query.WhereIf(input.ToDay.HasValue,
                    u => u.FoundedDate.HasValue && u.FoundedDate <= toDay);
            }
            
            return query;
        }


    }
}