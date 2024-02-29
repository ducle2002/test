using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Organizations;
using Abp.RealTime;
using Abp.UI;
using Yootek.Application;
using Yootek.AppManager.HomeMembers;
using Yootek.Authorization;
using Yootek.Authorization.Roles;
using Yootek.Authorization.Users;
using Yootek.Common.DataResult;
using Yootek.Common.Enum.Quanlydancu;
using Yootek.EntityDb;
using Yootek.Notifications;
using Yootek.Organizations;
using Yootek.QueriesExtension;
using Yootek.Services.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using static Yootek.Common.Enum.CommonENum;

namespace Yootek.Services
{
    public interface ICitizenAppService : IApplicationService
    {
        Task<object> GetAllCitizenAsync(GetAllCitizenInput input);

        Task<object> GetDetailCitizenVerification(long id);

        // Task<object> CreateListCitizenAsync(List<CitizenDto> input);
        Task<object> GetCitizenByIdAsync(long id);

        Task<object> GetAllAccountTenant();
        Task<object> GetAllUserTenant();
        Task<object> CreateOrUpdateCitizen(CreateOrUpdateInput input);
        Task<object> CreateCitizen(CreateCitizenByAdminInput input);
        Task<object> UpdateCitizen(UpdateCitizenByAdminInput input);
        Task<DataResult> DeleteCitizen(long id);
        Task<DataResult> UpdateStateCitizen(CitizenDto input);

        Task<DataResult> DeleteMultipleCitizen([FromBody] List<long> ids);

        Task<object> GetAllSmarthomeTenant();
        Task<object> GetAllSmarthomeTenantByUser(long UserId);
        Task<object> GetAllSmarthomeByTenant(GetAllCitizenInput input);
    }

    //[AbpAuthorize(PermissionNames.Pages_Citizens_List, PermissionNames.Pages_Citizens_Verifications,
    //    PermissionNames.Pages_Residents, PermissionNames.Pages_Residents_Verification)]
    [AbpAuthorize]
    public class CitizenAppService : YootekAppServiceBase, ICitizenAppService
    {
        private readonly IRepository<Citizen, long> _citizenRepos;
        private readonly IRepository<CitizenTemp, long> _citizenTempRepos;
        private readonly IRepository<HomeMember, long> _homeMemberRepos;
        private readonly IRepository<User, long> _userRepos;
        private readonly IRepository<SmartHome, long> _smartHomeRepos;
        private readonly IRepository<CityVote, long> _cityVoteRepos;
        private readonly IRepository<UserVote, long> _userVoteRepos;
        private readonly IRepository<Role, int> _roleRepos;
        private readonly IRepository<UserRole, long> _userRoleRepos;
        private readonly IRepository<OrganizationUnit, long> _organizationUnitRepository;
        private readonly IRepository<UserOrganizationUnit, long> _userOrganizationUnitRepository;
        private readonly IRepository<CitizenVerification, long> _citizenVerificationRepos;
        private readonly INotificationCommunicator _communicator;
        private readonly IOnlineClientManager _onlineClientManager;
        private readonly IRepository<AppOrganizationUnit, long> _appOrganizationUnitRepository;
        private readonly IHomeMemberManager _homeMemberManager;
        private readonly IAppNotifier _appNotifier;
        private readonly ICitizenListExcelExporter _citizenListExcelExporter;

        public CitizenAppService(
            IRepository<User, long> userRepos,
            IRepository<Citizen, long> citizenRepos,
            IRepository<HomeMember, long> homeMemberRepos,
            IRepository<CitizenTemp, long> citizenTempRepos,
            IRepository<SmartHome, long> smartHomeRepos,
            IRepository<CityVote, long> cityVoteRepos,
            IRepository<UserVote, long> userVoteRepos,
            IRepository<Role, int> roleRepos,
            IRepository<UserRole, long> userRoleRepos,
            IRepository<OrganizationUnit, long> organizationUnitRepository,
            IRepository<UserOrganizationUnit, long> userOrganizationUnitRepository,
            IRepository<CitizenVerification, long> citizenVerificationRepos,
            INotificationCommunicator communicator,
            IOnlineClientManager onlineClientManager,
            IRepository<AppOrganizationUnit, long> appOrganizationUnitRepository,
            IHomeMemberManager homeMemberManager,
            IAppNotifier appNotifier,
            ICitizenListExcelExporter citizenListExcelExporter
        )
        {
            _userRepos = userRepos;
            _citizenRepos = citizenRepos;
            _citizenTempRepos = citizenTempRepos;
            _smartHomeRepos = smartHomeRepos;
            _cityVoteRepos = cityVoteRepos;
            _userVoteRepos = userVoteRepos;
            _roleRepos = roleRepos;
            _userRoleRepos = userRoleRepos;
            _organizationUnitRepository = organizationUnitRepository;
            _userOrganizationUnitRepository = userOrganizationUnitRepository;
            _citizenVerificationRepos = citizenVerificationRepos;
            _homeMemberRepos = homeMemberRepos;
            _communicator = communicator;
            _onlineClientManager = onlineClientManager;
            _appOrganizationUnitRepository = appOrganizationUnitRepository;
            _homeMemberManager = homeMemberManager;
            _appNotifier = appNotifier;
            _citizenListExcelExporter = citizenListExcelExporter;
        }

        #region Citizen

        public async Task<object> CreateOrUpdateCitizen(CreateOrUpdateInput input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    long t1 = TimeUtils.GetNanoseconds();

                    input.TenantId = AbpSession.TenantId;
                    if (input.Id > 0)
                    {
                        //update
                        var updateData = await _citizenRepos.GetAsync(input.Id);
                        if (input.State != updateData.State)
                        {
                            if (updateData.State.Value == STATE_CITIZEN.ACCEPTED)
                            {
                                await _homeMemberManager.UpdateCitizenInHomeMember(updateData, true, AbpSession.TenantId);
                            }
                            else
                            {
                                await _homeMemberManager.UpdateCitizenInHomeMember(updateData, false, AbpSession.TenantId);
                            }
                        }

                        if (updateData != null)
                        {
                            input.MapTo(updateData);
                            //call back
                            await _citizenRepos.UpdateAsync(updateData);
                        }

                        mb.statisticMetris(t1, 0, "Ud_citizen");

                        var data = DataResult.ResultSuccess(updateData, "Update success !");
                        return data;
                    }
                    else
                    {
                        var insertInput = input.MapTo<Citizen>();

                        if (insertInput.CitizenCode == null)
                        {
                            insertInput.CitizenCode = GetUniqueCitizenCode();
                        }
                        else insertInput.CitizenCode = input.CitizenCode;

                        long id = await _citizenRepos.InsertAndGetIdAsync(insertInput);
                        if (insertInput.State.Value == STATE_CITIZEN.ACCEPTED)
                        {
                            await _homeMemberManager.UpdateCitizenInHomeMember(insertInput, true, AbpSession.TenantId);
                        }
                        else
                        {
                            await _homeMemberManager.UpdateCitizenInHomeMember(insertInput, false, AbpSession.TenantId);
                        }

                        insertInput.Id = id;
                        mb.statisticMetris(t1, 0, "is_citizen");
                        var data = DataResult.ResultSuccess(insertInput, "Insert success !");
                        return data;
                    }
                }
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        private async Task<CitizenTemp> HanleCheckCitizenInternal(CitizenDto input)
        {
            try
            {
                if (input.FullName != null)
                {
                    var query = (from ctemp in _citizenTempRepos.GetAll()
                                 where ctemp.ApartmentCode.Trim() == input.ApartmentCode.Trim()
                                       && ctemp.FullName.Trim().ToLower() == input.FullName.Trim().ToLower()
                                       && ctemp.BuildingId == input.BuildingId
                                       && ctemp.UrbanId == input.UrbanId
                                 select new CitizenTemp()
                                 {
                                     FullName = ctemp.FullName,
                                     Address = ctemp.Address,
                                     ApartmentCode = ctemp.ApartmentCode,
                                     BuildingCode = ctemp.BuildingCode,
                                     Email = ctemp.Email,
                                     Gender = ctemp.Gender,
                                     IdentityNumber = ctemp.IdentityNumber,
                                     Id = ctemp.Id,
                                     Nationality = ctemp.Nationality,
                                     PhoneNumber = ctemp.PhoneNumber,
                                     BirthYear = ctemp.BirthYear,
                                     RelationShip = ctemp.RelationShip,
                                     MemberNum = ctemp.MemberNum,
                                     CitizenCode = ctemp.CitizenCode,
                                     DateOfBirth = ctemp.DateOfBirth,
                                     IsVoter = ctemp.IsVoter,
                                     OrganizationUnitId = ctemp.OrganizationUnitId,
                                     Hometown = ctemp.Hometown
                                 }).FirstOrDefault();
                    return query;
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        [HttpPost]
        public async Task<object> CreateCitizen(CreateCitizenByAdminInput input)
        {
            try
            {
                Citizen insertInput = input.MapTo<Citizen>();
                insertInput.TenantId = AbpSession.TenantId;
                insertInput.State = STATE_CITIZEN.NEW;

                if (insertInput.CitizenCode == null)
                {
                    insertInput.CitizenCode = GetUniqueCitizenCode();
                }
                else insertInput.CitizenCode = input.CitizenCode;

                await _citizenRepos.InsertAsync(insertInput);
                // await _homeMemberManager.UpdateCitizenInHomeMember(insertInput, true, AbpSession.TenantId);
                await _homeMemberManager.UpdateCitizenInHomeMember(insertInput, false, AbpSession.TenantId);
                return DataResult.ResultSuccess(true, "Insert success !");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        [HttpPut]
        public async Task<object> UpdateCitizen(UpdateCitizenByAdminInput input)
        {
            try
            {
                input.TenantId = AbpSession.TenantId;
                Citizen? updateData = await _citizenRepos.GetAsync(input.Id);
                if (updateData != null)
                {
                    input.MapTo(updateData);
                    await _citizenRepos.UpdateAsync(updateData);
                }

                if (input.State != updateData.State)
                {
                    if (updateData.State.Value == STATE_CITIZEN.ACCEPTED)
                    {
                        await _homeMemberManager.UpdateCitizenInHomeMember(updateData, true, AbpSession.TenantId);
                    }
                    else
                    {
                        await _homeMemberManager.UpdateCitizenInHomeMember(updateData, false, AbpSession.TenantId);
                    }
                }

                return DataResult.ResultSuccess(true, "Update success !");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public Task<DataResult> DeleteCitizen(long id)
        {
            try
            {
                _citizenRepos.DeleteAsync(id);
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

        protected IQueryable<CitizenDto> QueryCitizen(GetAllCitizenInput input)
        {
            List<long> buIds = UserManager.GetAccessibleBuildingOrUrbanIds();

            var query = (from ci in _citizenRepos.GetAll()
                         join us in _userRepos.GetAll() on ci.AccountId equals us.Id into tb_us
                         from us in tb_us.DefaultIfEmpty()
                         select new CitizenDto()
                         {
                             Id = ci.Id,
                             PhoneNumber = ci.PhoneNumber != null ? ci.PhoneNumber : us.PhoneNumber,
                             Nationality = ci.Nationality,
                             FullName = ci.FullName,
                             IdentityNumber = ci.IdentityNumber,
                             ImageUrl = ci.ImageUrl != null ? ci.ImageUrl : us.ImageUrl,
                             Email = ci.Email != null ? ci.Email : (us.EmailAddress.Contains("yootek") ? us.EmailAddress : null),
                             Address = ci.Address,
                             DateOfBirth = ci.DateOfBirth,
                             AccountId = ci.AccountId,
                             Gender = ci.Gender,
                             IsVoter = ci.IsVoter,
                             State = ci.State,
                             ApartmentCode = ci.ApartmentCode,
                             Type = ci.Type,
                             TenantId = ci.TenantId,
                             OrganizationUnitId = ci.OrganizationUnitId,
                             RelationShip = ci.RelationShip,
                             CitizenCode = ci.CitizenCode,
                             MemberNum = ci.MemberNum,
                             Career = ci.Career,
                             UrbanId = ci.UrbanId,
                             BuildingId = ci.BuildingId,
                             CitizenTempId = ci.CitizenTempId,
                             IdentityImageUrls = ci.IdentityImageUrls,
                             CreationTime = ci.CreationTime,
                             BuildingName = _appOrganizationUnitRepository.GetAll().Where(x => x.Id == ci.BuildingId).Select(x => x.DisplayName).FirstOrDefault(),
                             UrbanName = _appOrganizationUnitRepository.GetAll().Where(x => x.Id == ci.UrbanId).Select(x => x.DisplayName).FirstOrDefault(),
                             HomeAddress = ci.HomeAddress,
                         })
                         .WhereIf(input.UrbanId.HasValue, x => x.UrbanId == input.UrbanId)
                         .WhereIf(input.BuildingId.HasValue, x => x.BuildingId == input.BuildingId)
                         .WhereIf(input.ApartmentCode != null, x => x.ApartmentCode == input.ApartmentCode)
                         .WhereByBuildingOrUrbanIf(!IsGranted(PermissionNames.Data_Admin), buIds)
                         .ApplySearchFilter(input.Keyword, x => x.FullName, x => x.Address, x => x.Email, x => x.ApartmentCode);

            switch (input.FormId)
            {
                case (int)GET_CITIZEN_FORMID.GET_ALL:
                    break;
                case (int)GET_CITIZEN_FORMID.GET_VERIFIED:
                    query = query.Where(x => x.State.Value == STATE_CITIZEN.ACCEPTED);
                    break;
                case (int)GET_CITIZEN_FORMID.GET_MATCH:
                    query = query.Where(x => x.State.Value == STATE_CITIZEN.MATCHCHECK);
                    break;
                case (int)GET_CITIZEN_FORMID.GET_UNVERIFIED:
                    query = query.Where(x => x.State.Value == STATE_CITIZEN.NEW ||
                                             x.State.Value == STATE_CITIZEN.MISMATCH
                                             || x.State.Value == STATE_CITIZEN.REFUSE ||
                                             x.State.Value == STATE_CITIZEN.EDITED);
                    break;
                case (int)GET_CITIZEN_FORMID.GET_REFUSE:
                    query = query.Where(x => x.State.Value == STATE_CITIZEN.REFUSE);
                    break;
                case (int)GET_CITIZEN_FORMID.GET_DISABLE:
                    query = query.Where(x => x.State.Value == STATE_CITIZEN.DISABLE);
                    break;
                case (int)GET_CITIZEN_FORMID.GET_NEW:
                    query = query.Where(x => x.State.Value == STATE_CITIZEN.NEW ||
                                             x.State.Value == STATE_CITIZEN.MISMATCH
                                             || x.State.Value == STATE_CITIZEN.MATCHCHECK);
                    break;
                default:
                    break;
            }


            DateTime fromDay = new DateTime(), toDay = new DateTime();

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

            if (input.State.HasValue) query = query.Where(x => x.State.Value == input.State);
            return query;
        }

        public async Task<object> GetAllCitizenAsync(GetAllCitizenInput input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var query = input.FormId == 7 ? QueryCitizen(input)
                        .ApplySort(input.OrderBy, input.SortBy)
                        .ApplySort(OrderByCitizen.CREATION_TIME, SortBy.DESC)
                        : QueryCitizen(input)
                        .ApplySort(input.OrderBy, input.SortBy)
                        .ApplySort(OrderByCitizen.APARTMENT_CODE);

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

        public async Task<object> GetCitizenCountStatistic()
        {
            try
            {
                List<long> buIds = UserManager.GetAccessibleBuildingOrUrbanIds();
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var count = await _citizenRepos.GetAll()
                        .WhereByBuildingOrUrbanIf(!IsGranted(PermissionNames.Data_Admin), buIds)
                        .CountAsync();
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

        public async Task<object> GetAllCitizenStatistic(GetStatisticCitizenInput input)
        {
            try
            {
                List<long> buIds = UserManager.GetAccessibleBuildingOrUrbanIds();
                long t1 = TimeUtils.GetNanoseconds();
                DateTime now = DateTime.Now;
                int currentMonth = now.Month;
                int currentYear = now.Year;

                Dictionary<string, ResultStatisticCitizen> dataResult =
                    new Dictionary<string, ResultStatisticCitizen>();

                switch (input.QueryCase)
                {
                    case QueryCaseCitizenStatistics.ByMonth:
                        if (currentMonth >= input.NumberRange)
                        {
                            for (int index = currentMonth - input.NumberRange + 1; index <= currentMonth; index++)
                            {
                                var result = new ResultStatisticCitizen();
                                var query = _citizenRepos.GetAll()
                                    .WhereByBuildingOrUrbanIf(!IsGranted(PermissionNames.Data_Admin), buIds)
                                    .AsQueryable();
                                result.CountNew = await query.Where(x => x.State.Value == STATE_CITIZEN.NEW ||
                                                                         x.State.Value == STATE_CITIZEN.MISMATCH
                                                                         || x.State.Value == STATE_CITIZEN.MATCHCHECK)
                                    .Where(x => x.CreationTime.Month == index && x.CreationTime.Year == currentYear)
                                    .CountAsync();
                                result.CountRejected = await query.Where(x => x.State.Value == STATE_CITIZEN.DISABLE)
                                    .Where(x => x.CreationTime.Month == index && x.CreationTime.Year == currentYear)
                                    .CountAsync();
                                result.CountAccepted = await query.Where(x => x.State.Value == STATE_CITIZEN.ACCEPTED)
                                    .Where(x => x.CreationTime.Month == index && x.CreationTime.Year == currentYear)
                                    .CountAsync();
                                result.CountTotal = await query.Where(x =>
                                    x.CreationTime.Month == index && x.CreationTime.Year == currentYear).CountAsync();
                                dataResult.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(index), result);
                            }
                        }
                        else
                        {
                            for (var index = 13 - (input.NumberRange - currentMonth); index <= 12; index++)
                            {
                                var result = new ResultStatisticCitizen();
                                var query = _citizenRepos.GetAll()
                                    .WhereByBuildingOrUrbanIf(!IsGranted(PermissionNames.Data_Admin), buIds)
                                    .AsQueryable();
                                result.CountNew = await query.Where(x => x.State.Value == STATE_CITIZEN.NEW ||
                                                                         x.State.Value == STATE_CITIZEN.MISMATCH
                                                                         || x.State.Value == STATE_CITIZEN.MATCHCHECK)
                                    .Where(x => x.CreationTime.Month == index && x.CreationTime.Year == currentYear - 1)
                                    .CountAsync();
                                result.CountRejected = await query.Where(x => x.State.Value == STATE_CITIZEN.DISABLE)
                                    .Where(x => x.CreationTime.Month == index && x.CreationTime.Year == currentYear -1)
                                    .CountAsync();
                                result.CountAccepted = await query.Where(x => x.State.Value == STATE_CITIZEN.ACCEPTED)
                                    .Where(x => x.CreationTime.Month == index && x.CreationTime.Year == currentYear - 1)
                                    .CountAsync();
                                result.CountTotal = await query.Where(x =>
                                        x.CreationTime.Month == index && x.CreationTime.Year == currentYear -1)
                                    .CountAsync();
                                dataResult.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(index), result);
                            }

                            for (var index = 1; index <= currentMonth; index++)
                            {
                                var result = new ResultStatisticCitizen();
                                var query = _citizenRepos.GetAll()
                                    .WhereByBuildingOrUrbanIf(!IsGranted(PermissionNames.Data_Admin), buIds)
                                    .AsQueryable();
                                result.CountNew = await query.Where(x => x.State.Value == STATE_CITIZEN.NEW ||
                                                                         x.State.Value == STATE_CITIZEN.MISMATCH
                                                                         || x.State.Value == STATE_CITIZEN.MATCHCHECK)
                                    .Where(x => x.CreationTime.Month == index && x.CreationTime.Year == currentYear)
                                    .CountAsync();
                                result.CountRejected = await query.Where(x => x.State.Value == STATE_CITIZEN.DISABLE)
                                    .Where(x => x.CreationTime.Month == index && x.CreationTime.Year == currentYear)
                                    .CountAsync();
                                result.CountAccepted = await query.Where(x => x.State.Value == STATE_CITIZEN.ACCEPTED)
                                    .Where(x => x.CreationTime.Month == index && x.CreationTime.Year == currentYear)
                                    .CountAsync();
                                result.CountTotal = await query.Where(x =>
                                    x.CreationTime.Month == index && x.CreationTime.Year == currentYear).CountAsync();
                                dataResult.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(index), result);
                            }
                        }

                        break;
                }

                var data = DataResult.ResultSuccess(dataResult, "Get success!");
                mb.statisticMetris(t1, 0, "get_statistic_citizen");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        [Obsolete]
        public async Task<object> GetDetailCitizenVerification(long id)
        {
            try
            {
                var citizen = await _citizenRepos.GetAsync(id);
                if (citizen.UrbanId.HasValue) citizen.UrbanCode = (await _appOrganizationUnitRepository.FirstOrDefaultAsync(citizen.UrbanId.Value)).ProjectCode;
                else citizen.UrbanCode = null;
                if (citizen.BuildingId.HasValue) citizen.BuildingCode = (await _appOrganizationUnitRepository.FirstOrDefaultAsync(citizen.BuildingId.Value)).ProjectCode;
                else citizen.BuildingCode = null;
                if (citizen == null)
                {
                    var data = DataResult.ResultError("", "Get success!");
                    return data;
                }
                else
                {
                    var result = new GetCitizenVerifiedDto();
                    result.Citizen = citizen.MapTo<CitizenDto>();
                    var czt = await _citizenTempRepos.FirstOrDefaultAsync(x => x.Id == citizen.CitizenTempId);
                    if (czt != null)
                    {
                        result.CitizenTemp = czt.MapTo<CitizenTempDto>();
                        var czv = await _citizenVerificationRepos.GetAll().Where(x => x.CitizenTempId == czt.Id)
                            .OrderBy(x => x.Id).LastOrDefaultAsync();
                        if (czv != null)
                        {
                            result.CitizenVerification = czv.MapTo<CitizenVerificationDto>();
                        }
                    }

                    var data = DataResult.ResultSuccess(result, "Get success!");
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

        public async Task<object> GetAllUserTenant()
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    // var role = await _roleRepos.FirstOrDefaultAsync(x => x.Name == StaticRoleNames.Tenants.DefaultUser);
                    var query = (from us in _userRepos.GetAll()

                                 select new AccountDto()
                                 {
                                     FullName = us.FullName,
                                     ImageUrl = us.ImageUrl,
                                     Id = us.Id,
                                     UserName = us.UserName,

                                 }).AsQueryable();

                    var result = await query.ToListAsync();
                    var data = DataResult.ResultSuccess(result, "Get success!");
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

        public async Task<List<AccountDto>> GetAllUserTenantNotVertify(GetAllUserTenantNotVertifyInput input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var query = (from us_role in _userRoleRepos.GetAll()
                                 join us in _userRepos.GetAll() on us_role.UserId equals us.Id into tb_us
                                 from us in tb_us.DefaultIfEmpty()
                                 select new AccountDto()
                                 {
                                     Name = us.Name,
                                     FullName = us.FullName,
                                     ImageUrl = us.ImageUrl,
                                     Id = us.Id,
                                     UserName = us.UserName,
                                     PhoneNumber = (from ctz in _citizenRepos.GetAll()
                                                    where ctz.AccountId == us.Id
                                                    select ctz.PhoneNumber).FirstOrDefault(),
                                     State = (from ctz in _citizenRepos.GetAll()
                                              where ctz.AccountId == us.Id
                                              select ctz.State).FirstOrDefault()
                                 })
                                 .Where(x => x.State != STATE_CITIZEN.ACCEPTED)
                                 .AsQueryable();

                    var result = await query
                        .ApplySort(input.OrderBy, input.SortBy)
                        .ApplySort(OrderByUserTenantNotVertify.NAME)
                        .ToListAsync();
                    return result;
                }
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<List<AccountDto>> GetAllUserTenantVertify(GetAllUserTenantVertifyInput input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var query = (from us_role in _userRoleRepos.GetAll()
                                 join us in _userRepos.GetAll() on us_role.UserId equals us.Id into tb_us
                                 from us in tb_us.DefaultIfEmpty()
                                 select new AccountDto()
                                 {
                                     Name = us.Name,
                                     FullName = us.FullName,
                                     ImageUrl = us.ImageUrl,
                                     Id = us.Id,
                                     UserName = us.UserName,
                                     PhoneNumber = (from ctz in _citizenRepos.GetAll()
                                                    where ctz.AccountId == us.Id
                                                    select ctz.PhoneNumber).FirstOrDefault(),
                                     State = (from ctz in _citizenRepos.GetAll()
                                              where ctz.AccountId == us.Id
                                              select ctz.State).FirstOrDefault()
                                 })
                                 .Where(x => x.State == STATE_CITIZEN.ACCEPTED)
                                 //.ApplySearchFilter(input.Keyword, x => x.FullName, x => x.UserName, x => x.PhoneNumber)
                                 .AsQueryable();

                    var result = await query
                        .ApplySort(input.OrderBy, input.SortBy)
                        .ApplySort(OrderByUserTenantVertify.NAME)
                        .ToListAsync();
                    return result;
                }
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> GetAllAccountTenant()
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var query = (from us in _userRepos.GetAll()
                                 select new AccountDto()
                                 {
                                     FullName = us.FullName,
                                     ImageUrl = us.ImageUrl,
                                     Id = us.Id,
                                     UserName = us.UserName
                                 }).AsQueryable();

                    var result = await query.ToListAsync();
                    var data = DataResult.ResultSuccess(result, "Get success!");
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

        public async Task<object> GetCitizenByIdAsync(long id)
        {
            try
            {
                var result = await _citizenRepos.GetAsync(id);
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

        public async Task<object> GetAllSmarthomeTenant()
        {
            try
            {
                var result = await _smartHomeRepos.GetAllListAsync();

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

        public async Task<object> GetAllSmarthomeTenantByUser(long userId)
        {
            try
            {
                var query = (from sm in _smartHomeRepos.GetAll()
                             join mb in _homeMemberRepos.GetAll() on sm.SmartHomeCode equals mb.SmartHomeCode into tb_mb
                             from mb in tb_mb.DefaultIfEmpty()
                             where sm.TenantId == AbpSession.TenantId && mb.UserId == userId
                             select new SmarthomeTenantDto
                             {
                                 Id = sm.Id,
                                 ImageUrl = sm.ImageUrl,
                                 Name = sm.Name,
                                 SmarthomeCode = sm.SmartHomeCode
                             }).AsQueryable();

                var result = await query.ToListAsync();

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

        public async Task<object> GetAllSmarthome(GetAllCitizenInput input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var result = (from sm in _smartHomeRepos.GetAll()
                                  select new GetAllSmarthomeDto
                                  {
                                      Id = sm.Id,
                                      ApartmentCode = sm.ApartmentCode,
                                      Name = sm.Name,
                                      ApartmentAreas = sm.ApartmentAreas,
                                      OrganizationUnitId = sm.OrganizationUnitId
                                  }).ToList();
                    var data = DataResult.ResultSuccess(result, "Get success!");
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


        public async Task<object> GetAllSmarthomeByTenant(GetAllCitizenInput input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    // var ouUI = await _userOrganizationUnitRepository.GetAll()
                    // .Where(x => x.UserId == AbpSession.UserId)
                    // .Select(x => x.OrganizationUnitId)
                    // .ToListAsync();
                    var query = (from sm in _smartHomeRepos.GetAll()
                                 where (sm.ApartmentCode != null)
                                 select new SmarthomeByTenantDto
                                 {
                                     Id = sm.Id,
                                     ApartmentCode = sm.ApartmentCode,
                                     Name = sm.Name,
                                     HouseDetail = sm.HouseDetail,
                                     BuildingCode = sm.BuildingCode,
                                     Citizens = (from citizen in _citizenTempRepos.GetAll()
                                                 where citizen.ApartmentCode.ToUpper() == sm.ApartmentCode.ToUpper()
                                                 select new CitizenRole
                                                 {
                                                     Name = citizen.FullName,
                                                     Type = citizen.Type,
                                                     RelationShip = citizen.RelationShip,
                                                     Nationality = citizen.Nationality,
                                                     Contact = citizen.PhoneNumber,
                                                     OwnerGeneration = citizen.OwnerGeneration > 0 ? citizen.OwnerGeneration : 0,
                                                     OwnerId = citizen.OwnerId
                                                 })
                                         .ToList(),
                                     BuildingId = sm.BuildingId,
                                     ApartmentAreas = sm.ApartmentAreas,
                                     OrganizationUnitId = sm.OrganizationUnitId,
                                     UrbanId = sm.UrbanId,
                                 })
                        .WhereIf(input.BuildingId.HasValue, u => u.BuildingId == input.BuildingId)
                        //.WhereIf(input.UrbanId.HasValue, u => u.UrbanId == input.UrbanId)
                        // .Where(x => ouUI.Contains(x.OrganizationUnitId.Value))
                        .WhereIf(input.ApartmentCode != null, x => x.ApartmentCode == input.ApartmentCode)
                        .WhereIf(input.BuildingCode != null, u => u.BuildingCode == input.BuildingCode)
                        .WhereIf(input.OrganizationUnitId != null,
                            u => u.OrganizationUnitId == input.OrganizationUnitId)
                        .AsQueryable();

                    if (input.Keyword != null)
                    {
                        var citizen = (from cz in _citizenTempRepos.GetAll()
                                       where cz.ApartmentCode != null && cz.FullName != null
                                       select cz).AsQueryable();
                        var listKey = input.Keyword.Split('+');
                        if (listKey != null)
                        {
                            foreach (var key in listKey)
                            {
                                var citizenTemp = citizen.Where(x => x.FullName.Contains(key))
                                    .Select(x => x.ApartmentCode).Take(10).ToList();
                                query = query.Where(u =>
                                    (u.ApartmentCode.ToLower().Contains(key.ToLower()) || u.HouseDetail.Contains(key) ||
                                     citizenTemp.Contains(u.ApartmentCode)));
                            }
                        }
                    }

                    //query = query.WhereIf(input.BuildingCode.Length > 0, x => x.BuildingCode.Contains(input.BuildingCode));
                    query = query.OrderBy(x => x.ApartmentCode);

                    var result = await query.Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();

                    foreach (var apt in result)
                    {
                        var highestGen = apt.Citizens.Select(x => x.OwnerGeneration).Max() ?? 0;
                        apt.CurrentCitizenCount = apt.Citizens.Where(x => x.OwnerGeneration == highestGen).Count();
                    }

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


        public async Task<DataResult> UpdateStateCitizen(CitizenDto input)
        {
            try
            {


                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    long t1 = TimeUtils.GetNanoseconds();
                    //update
                    var updateData = _citizenRepos.Get(input.Id);
                    if (updateData.State == input.State) return DataResult.ResultFail("State no change!");
                    updateData.State = input.State;
                    updateData.Type = input.Type;
                    await _homeMemberManager.UpdateCitizenInHomeMember(input, input.State == STATE_CITIZEN.ACCEPTED,
                         AbpSession.TenantId);
                    await _citizenRepos.UpdateAsync(updateData);
                    var citizenInternal = await HanleCheckCitizenInternal(input);
                    if (input.State == STATE_CITIZEN.ACCEPTED)
                    {
                        if (input.CitizenTempId.HasValue && (await _citizenTempRepos.CountAsync(x => x.Id == input.CitizenTempId) > 0))
                        {
                            CitizenTemp citizenTemp = await _citizenTempRepos.GetAsync(input.CitizenTempId.Value);
                            citizenTemp.FullName = input.FullName;
                            citizenTemp.Address = input.Address;
                            citizenTemp.ApartmentCode = input.ApartmentCode;
                            citizenTemp.BirthYear = input.BirthYear.HasValue ? input.BirthYear.Value : input.DateOfBirth.Value.Year;
                            citizenTemp.BuildingCode = input.BuildingCode;
                            citizenTemp.BuildingId = input.BuildingId;
                            citizenTemp.Career = input.Career;
                            citizenTemp.CitizenCode = input.CitizenCode;
                            citizenTemp.DateOfBirth = input.DateOfBirth;
                            citizenTemp.Email = input.Email;
                            citizenTemp.Gender = input.Gender;
                            citizenTemp.IdentityNumber = input.IdentityNumber;
                            citizenTemp.ImageUrl = input.ImageUrl;
                            citizenTemp.MemberNum = input.MemberNum;
                            citizenTemp.State = (int?)input.State;
                            citizenTemp.Nationality = input.Nationality;
                            citizenTemp.OtherPhones = input.OtherPhones;
                            citizenTemp.IsStayed = input.IsStayed ?? true;
                            citizenTemp.IsVoter = input.IsVoter;
                            citizenTemp.OrganizationUnitId = input.OrganizationUnitId;
                            citizenTemp.Type = input.Type ?? 0;
                            citizenTemp.PhoneNumber = input.PhoneNumber;
                            citizenTemp.RelationShip = input.RelationShip;
                            citizenTemp.TenantId = input.TenantId;
                            citizenTemp.UrbanCode = input.UrbanCode;
                            citizenTemp.AccountId = input.AccountId;
                            citizenTemp.UrbanId = input.UrbanId;
                            citizenTemp.Hometown = input.HomeAddress;
                            await _citizenTempRepos.UpdateAsync(citizenTemp);
                            if (!updateData.CitizenTempId.HasValue) updateData.CitizenTempId = citizenTemp.Id;
                            await CurrentUnitOfWork.SaveChangesAsync();
                        }
                        else if (citizenInternal != null)
                        {
                            citizenInternal.FullName = input.FullName;
                            citizenInternal.Address = input.Address;
                            citizenInternal.ApartmentCode = input.ApartmentCode;
                            citizenInternal.BirthYear = input.BirthYear.HasValue ? input.BirthYear.Value : input.DateOfBirth.Value.Year;
                            citizenInternal.BuildingCode = input.BuildingCode;
                            citizenInternal.BuildingId = input.BuildingId;
                            citizenInternal.Career = input.Career;
                            citizenInternal.CitizenCode = input.CitizenCode;
                            citizenInternal.DateOfBirth = input.DateOfBirth;
                            citizenInternal.Email = input.Email;
                            citizenInternal.Gender = input.Gender;
                            citizenInternal.IdentityNumber = input.IdentityNumber;
                            citizenInternal.ImageUrl = input.ImageUrl;
                            citizenInternal.MemberNum = input.MemberNum;
                            citizenInternal.State = (int?)input.State;
                            citizenInternal.Nationality = input.Nationality;
                            citizenInternal.OtherPhones = input.OtherPhones;
                            citizenInternal.IsStayed = input.IsStayed ?? true;
                            citizenInternal.IsVoter = input.IsVoter;
                            citizenInternal.OrganizationUnitId = input.OrganizationUnitId;
                            citizenInternal.Type = input.Type ?? 0;
                            citizenInternal.PhoneNumber = input.PhoneNumber;
                            citizenInternal.RelationShip = input.RelationShip;
                            citizenInternal.TenantId = input.TenantId;
                            citizenInternal.UrbanCode = input.UrbanCode;
                            citizenInternal.AccountId = input.AccountId;
                            citizenInternal.UrbanId = input.UrbanId;
                            citizenInternal.Hometown = input.HomeAddress;
                            await _citizenTempRepos.UpdateAsync(citizenInternal);
                            if (!updateData.CitizenTempId.HasValue) updateData.CitizenTempId = citizenInternal.Id;
                            await CurrentUnitOfWork.SaveChangesAsync();
                        }
                        else
                        {

                            CitizenTemp citizenTemp = new()
                            {
                                FullName = input.FullName,
                                Address = input.Address,
                                ApartmentCode = input.ApartmentCode,
                                BirthYear = input.BirthYear ?? input.DateOfBirth.Value.Year,
                                BuildingCode = input.BuildingCode,
                                BuildingId = input.BuildingId,
                                Career = input.Career,
                                CitizenCode = input.CitizenCode,
                                DateOfBirth = input.DateOfBirth,
                                Email = input.Email,
                                Gender = input.Gender,
                                IdentityNumber = input.IdentityNumber,
                                ImageUrl = input.ImageUrl,
                                MemberNum = input.MemberNum,
                                State = (int?)(input.State ?? STATE_CITIZEN.NEW),
                                Nationality = input.Nationality,
                                OtherPhones = input.OtherPhones,
                                IsStayed = input.IsStayed ?? true,
                                IsVoter = input.IsVoter,
                                OrganizationUnitId = input.OrganizationUnitId,
                                Type = input.Type ?? 0,
                                PhoneNumber = input.PhoneNumber,
                                RelationShip = input.RelationShip,
                                TenantId = input.TenantId,
                                UrbanCode = input.UrbanCode,
                                AccountId = input.AccountId,
                                UrbanId = input.UrbanId,
                                Hometown = input.HomeAddress
                            };
                            var olderOwners = _citizenTempRepos.GetAll()
                                .Where(x => x.RelationShip == RELATIONSHIP.Contractor && x.ApartmentCode == updateData.ApartmentCode && x.Id != updateData.Id && x.IsStayed == citizenTemp.IsStayed)
                                .OrderByDescending(x => x.OwnerGeneration)
                                .ToList();
                            var olderOwner = olderOwners.FirstOrDefault();
                            if (input.RelationShip == RELATIONSHIP.Contractor && olderOwner != null)

                                citizenTemp.OwnerGeneration = olderOwner.OwnerGeneration + 1;

                            else citizenTemp.OwnerGeneration = 1;
                            var id = await _citizenTempRepos.InsertAndGetIdAsync(citizenTemp);
                            updateData.CitizenTempId = id;
                            await CurrentUnitOfWork.SaveChangesAsync();
                        }
                    }
                    await FireNotificationStateCitizen(updateData);
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

        public Task<DataResult> DeleteMultipleCitizen([FromBody] List<long> ids)
        {
            try
            {
                if (ids.Count == 0) return Task.FromResult(DataResult.ResultError("Err", "input empty"));
                var tasks = new List<Task>();
                foreach (var id in ids)
                {
                    var tk = DeleteCitizen(id);
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

        #endregion

        #region Common

        protected async Task AddMemberToFramily(Citizen user)
        {
            try
            {
                var memberIds = await (from mb in _citizenTempRepos.GetAll()
                                       where mb.ApartmentCode == user.ApartmentCode
                                       select mb.Id).ToListAsync();
                if (memberIds != null)
                {
                    var citizens = (from ctz in _citizenRepos.GetAll()
                                    where memberIds.Contains(ctz.CitizenTempId.Value) && ctz.AccountId != null
                                    select ctz.AccountId.Value).ToList();
                    if (citizens != null)
                    {
                        var framily = _homeMemberRepos.GetAllListAsync(x => citizens.Contains(x.UserId.Value));
                    }
                }
            }
            catch (Exception e)
            {
            }
        }

        private async Task FireNotificationStateCitizen(Citizen citizen)
        {
            try
            {
                string detailUrlApp, detailUrlWA;
                switch (citizen.State.Value)
                {
                    case STATE_CITIZEN.ACCEPTED:
                        detailUrlApp = "yoolife://app/verify-citizen";
                        detailUrlWA = $"/citizens/verify/id={citizen.Id}";
                        var messageAccept = new UserMessageNotificationDataBase(
                            AppNotificationAction.StateVerifyCitizen,
                            AppNotificationIcon.StateVerifyCitizenSuccess,
                            TypeAction.Detail,
                            "Thông tin cư dân của bạn đã được xác minh thành công!",
                            detailUrlApp,
                            detailUrlWA,
                            citizen.ImageUrl ?? "",
                            "",
                            0

                        );
                        await _appNotifier.SendUserMessageNotifyFullyAsync(
                            "Thông báo xác minh cư dân",
                            "Thông tin cư dân của bạn đã được xác minh thành công!",
                            detailUrlApp,
                            detailUrlWA,
                            new UserIdentifier[] { new UserIdentifier(citizen.TenantId, citizen.CreatorUserId.Value) },
                            messageAccept);
                        break;
                    case STATE_CITIZEN.DISABLE:
                        detailUrlApp = "yoolife://app/verify-citizen";
                        detailUrlWA = $"/citizens/verify/id={citizen.Id}";
                        var messageDisable = new UserMessageNotificationDataBase(
                            AppNotificationAction.StateVerifyCitizen,
                            AppNotificationIcon.StateVerifyCitizenDenied,
                            TypeAction.Detail,
                            "Thông tin xác minh cư dân của bạn đã bị từ chối. Nhấn để xem chi tiết !",
                            detailUrlApp,
                            detailUrlWA,
                            citizen.ImageUrl ?? "",
                            "",
                            0
                        );
                        await _appNotifier.SendUserMessageNotifyFullyAsync(
                            "Thông báo xác minh cư dân",
                            "Thông tin xác minh cư dân của bạn đã bị từ chối. Nhấn để xem chi tiết !",
                            detailUrlApp,
                            detailUrlWA,
                            new UserIdentifier[] { new UserIdentifier(citizen.TenantId, citizen.CreatorUserId.Value) },
                            messageDisable);
                        break;
                    case STATE_CITIZEN.REFUSE:
                        detailUrlApp = "yoolife://app/verify-citizen";
                        detailUrlWA = $"/citizens/verify?id={citizen.Id}";
                        var messageRefuse = new UserMessageNotificationDataBase(
                            AppNotificationAction.StateVerifyCitizen,
                            AppNotificationIcon.StateVerifyCitizenRefuse,
                            TypeAction.Detail,
                            "Thông tin xác minh cư dân của bạn được yêu cầu chỉnh sửa. Hãy bổ sung thêm !",
                            detailUrlApp,
                            detailUrlWA,
                            citizen.ImageUrl ?? "",
                            "",
                            0

                        );
                        await _appNotifier.SendUserMessageNotifyFullyAsync(
                            "Thông báo xác minh cư dân",
                            "Thông tin xác minh cư dân của bạn được yêu cầu chỉnh sửa. Hãy bổ sung thêm !",
                            detailUrlApp,
                            detailUrlWA,
                            new UserIdentifier[] { new UserIdentifier(citizen.TenantId, citizen.CreatorUserId.Value) },
                            messageRefuse);
                        break;
                    default:
                        break;
                }
            }
            catch
            {
            }
        }

        #endregion

        public async Task<object> ExportCitizenToExcel(ExportExcelInput input)
        {
            try
            {
                List<long> buIds = UserManager.GetAccessibleBuildingOrUrbanIds();
                var query = (from ci in _citizenRepos.GetAll()
                             join us in _userRepos.GetAll() on ci.AccountId equals us.Id into tb_us
                             from us in tb_us.DefaultIfEmpty()
                             select new CitizenDto()
                             {
                                 Id = ci.Id,
                                 PhoneNumber = ci.PhoneNumber != null ? ci.PhoneNumber : us.PhoneNumber,
                                 Nationality = ci.Nationality,
                                 FullName = ci.FullName,
                                 IdentityNumber = ci.IdentityNumber,
                                 ImageUrl = ci.ImageUrl != null ? ci.ImageUrl : us.ImageUrl,
                                 Email = ci.Email != null ? ci.Email : us.EmailAddress,
                                 Address = ci.Address,
                                 DateOfBirth = ci.DateOfBirth,
                                 AccountId = ci.AccountId,
                                 Gender = ci.Gender,
                                 IsVoter = ci.IsVoter,
                                 State = ci.State,
                                 ApartmentCode = ci.ApartmentCode,
                                 Type = ci.Type,
                                 BuildingCode = ci.BuildingCode,
                                 TenantId = ci.TenantId,
                                 OrganizationUnitId = ci.OrganizationUnitId,
                                 RelationShip = ci.RelationShip,
                                 CitizenCode = ci.CitizenCode,
                                 MemberNum = ci.MemberNum,
                                 Career = ci.Career,
                                 BuildingId = ci.BuildingId,
                                 UrbanId = ci.UrbanId,
                                 HomeAddress = ci.HomeAddress,
                             })
                    .WhereByBuildingOrUrbanIf(!IsGranted(PermissionNames.Data_Admin), buIds)
                    .WhereIf(input.Ids != null && input.Ids.Count > 0, x => input.Ids.Contains(x.Id))
                    .AsQueryable();
                switch (input.FormId)
                {
                    case (int)GET_CITIZEN_FORMID.GET_ALL:
                        break;
                    case (int)GET_CITIZEN_FORMID.GET_VERIFIED:
                        query = query.Where(x => x.State.Value == STATE_CITIZEN.ACCEPTED);
                        break;
                    case (int)GET_CITIZEN_FORMID.GET_MATCH:
                        query = query.Where(x => x.State.Value == STATE_CITIZEN.MATCHCHECK);
                        break;
                    case (int)GET_CITIZEN_FORMID.GET_UNVERIFIED:
                        query = query.Where(x => x.State.Value == STATE_CITIZEN.NEW ||
                                                 x.State.Value == STATE_CITIZEN.MISMATCH
                                                 || x.State.Value == STATE_CITIZEN.REFUSE ||
                                                 x.State.Value == STATE_CITIZEN.EDITED);
                        break;
                    case (int)GET_CITIZEN_FORMID.GET_REFUSE:
                        query = query.Where(x => x.State.Value == STATE_CITIZEN.REFUSE);
                        break;
                    case (int)GET_CITIZEN_FORMID.GET_DISABLE:
                        query = query.Where(x => x.State.Value == STATE_CITIZEN.DISABLE);
                        break;
                    case (int)GET_CITIZEN_FORMID.GET_NEW:
                        query = query.Where(x => x.State.Value == STATE_CITIZEN.NEW ||
                                                 x.State.Value == STATE_CITIZEN.MISMATCH
                                                 || x.State.Value == STATE_CITIZEN.MATCHCHECK);
                        break;
                    default:
                        break;
                }
                var citizens = await query.ToListAsync();
                var result = _citizenListExcelExporter.ExportToFile(citizens);
                return DataResult.ResultSuccess(result, "Export Success");
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> ImportCitizenExcel([FromForm] ImportCitizenExcelDto input)
        {
            try
            {
                var file = input.Form;

                var fileName = file.FileName;
                var fileExt = Path.GetExtension(fileName);
                if (fileExt != ".xlsx" && fileExt != ".xls")
                {
                    return DataResult.ResultError("File not support", "Error");
                }

                var filePath = Path.GetRandomFileName() + fileExt;
                var stream = File.Create(filePath);
                await file.CopyToAsync(stream);

                var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets.First();

                var rowCount = worksheet.Dimension.End.Row;
                var colCount = worksheet.Dimension.End.Column;

                for (var row = 2; row <= rowCount; row++)
                {
                }

                await stream.DisposeAsync();
                stream.Close();
                File.Delete(filePath);

                return DataResult.ResultSuccess("Success");
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