using Abp;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Yootek.Authorization.Users;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Yootek.Notifications;
using Yootek.Organizations;
using Yootek.Services.Dto;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace Yootek.Services
{
    public interface IUserCitizenAppService : IApplicationService
    {
        Task<object> CreateOrUpdateUserVote(UserVoteDto input);
        Task<object> CreateOrUpdateCitizen(CreateOrUpdateInput input);
        Task<DataResult> GetFamilyInfo(string apartmentCode);
        Task<DataResult> AddMemberToFamily(AddMemeberInput input);
        Task<object> GetAllCityVotes(GetAllCityVoteInput input);
        Task<object> GetAllCitizenApartment(GetAllCitizenApartment input);
        Task<object> CreateCitizen(CreateCitizenByUserInput input);
        Task<object> UpdateCitizen(UpdateCitizenByUserInput input);
        Task<object> GetCitizenInfo();
        Task<DataResult> GetInformationOfCitizen();
    }

    [AbpAuthorize]
    public class UserCitizenAppService : YootekAppServiceBase, IUserCitizenAppService
    {
        private readonly IRepository<Citizen, long> _citizenRepos;
        private readonly IRepository<Apartment, long> _apartmentRepository;
        private readonly IRepository<CitizenTemp, long> _citizenTempRepos;
        private readonly IRepository<User, long> _userRepos;
        private readonly IRepository<SmartHome, long> _smartHomeRepos;
        private readonly IRepository<HomeMember, long> _memberRepos;
        private readonly IRepository<CityVote, long> _cityVoteRepos;
        private readonly IRepository<UserVote, long> _userVoteRepos;
        private readonly IRepository<AppOrganizationUnit, long> _organizationUnitRepository;
        private readonly IRepository<UserOrganizationUnit, long> _userOrganizationUnitRepository;
        private readonly IRepository<CitizenVerification, long> _citizenVerificationRepos;
        private readonly IAppNotifier _appNotifier;
        private readonly UserStore _store;

        public UserCitizenAppService(
          IRepository<User, long> userRepos,
          IRepository<Citizen, long> citizenRepos,
          IRepository<Apartment, long> apartmentRepository,
          IRepository<CitizenTemp, long> citizenTempRepos,
          IRepository<SmartHome, long> smartHomeRepos,
          IRepository<HomeMember, long> memberRepos,
          IRepository<CityVote, long> cityVoteRepos,
          IRepository<UserVote, long> userVoteRepos,
          IRepository<AppOrganizationUnit, long> organizationUnitRepository,
          IRepository<UserOrganizationUnit, long> userOrganizationUnitRepository,
          IRepository<CitizenVerification, long> citizenVerificationRepos,
          IAppNotifier appNotifier,
          UserStore store
            )
        {
            _userRepos = userRepos;
            _citizenRepos = citizenRepos;
            _apartmentRepository = apartmentRepository;
            _citizenTempRepos = citizenTempRepos;
            _smartHomeRepos = smartHomeRepos;
            _memberRepos = memberRepos;
            _cityVoteRepos = cityVoteRepos;
            _userVoteRepos = userVoteRepos;
            _organizationUnitRepository = organizationUnitRepository;
            _userOrganizationUnitRepository = userOrganizationUnitRepository;
            _citizenVerificationRepos = citizenVerificationRepos;
            _appNotifier = appNotifier;
            _store = store;
        }

        #region Citizen
        public async Task<object> GetAllCitizenApartment(GetAllCitizenApartment input)
        {
            try
            {
                IQueryable<CitizenDto> query =
                    (from citizen in _citizenRepos.GetAll()
                     join a in _apartmentRepository.GetAll() on citizen.ApartmentCode equals a.ApartmentCode into
                         tb_a
                     from apartment in tb_a.DefaultIfEmpty()
                     select new CitizenDto()
                     {
                         Id = citizen.Id,
                         TenantId = citizen.TenantId,
                         FullName = citizen.FullName,
                         AccountId = citizen.AccountId,
                         Address = citizen.Address,
                         ApartmentCode = citizen.ApartmentCode,
                         BirthYear = citizen.BirthYear,
                         Career = citizen.Career,
                         CitizenCode = citizen.CitizenCode,
                         CitizenTempId = citizen.CitizenTempId,
                         DateOfBirth = citizen.DateOfBirth,
                         Email = citizen.Email,
                         Gender = citizen.Gender,
                         IdentityNumber = citizen.IdentityNumber,
                         ImageUrl = citizen.ImageUrl,
                         IsStayed = citizen.IsStayed,
                         IsVoter = citizen.IsVoter,
                         MemberNum = citizen.MemberNum,
                         Nationality = citizen.Nationality,
                         OrganizationUnitId = citizen.OrganizationUnitId,
                         OtherPhones = citizen.OtherPhones,
                         PhoneNumber = citizen.PhoneNumber,
                         RelationShip = citizen.RelationShip,
                         State = citizen.State,
                         Type = citizen.Type,
                         ApartmentId = apartment.Id,
                         ApartmentName = apartment.Name,
                         UrbanCode = citizen.UrbanCode,
                         UrbanId = citizen.UrbanId,
                         UrbanName = _organizationUnitRepository.GetAll().Where(i => i.Id == citizen.UrbanId)
                             .Select(x => x.DisplayName).FirstOrDefault(),
                         BuildingCode = citizen.BuildingCode,
                         BuildingId = citizen.BuildingId,
                         BuildingName = _organizationUnitRepository.GetAll()
                             .Where(i => i.Code == citizen.BuildingCode)
                             .Select(x => x.DisplayName).FirstOrDefault(),
                         HomeAddress = citizen.HomeAddress,
                     }
                    )
                    .Where(x => x.AccountId == AbpSession.UserId)
                    .WhereIf(input.State.HasValue, x => (int)x.State == input.State).AsQueryable();
                var listCitizenApartment = query.ToList();
                return DataResult.ResultSuccess(listCitizenApartment, "Get success!", listCitizenApartment.Count);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message);
                throw;
            }
        }
        public async Task<DataResult> GetFamilyInfo(string apartmentCode)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var query = (from mb in _memberRepos.GetAll()
                             join cz in _citizenRepos.GetAll() on mb.UserId equals cz.AccountId
                             join us in _userRepos.GetAll() on mb.UserId equals us.Id
                             where mb.ApartmentCode == apartmentCode && cz.State == STATE_CITIZEN.ACCEPTED
                             select new FamilyDto()
                             {
                                 ApartmentCode = cz.ApartmentCode,
                                 AccountId = cz.AccountId,
                                 Address = cz.Address,
                                 BirthYear = cz.BirthYear,
                                 BuildingCode = cz.BuildingCode,
                                 DateOfBirth = cz.DateOfBirth,
                                 Email = cz.Email,
                                 FullName = cz.FullName,
                                 Gender = cz.Gender,
                                 RelationShip = cz.RelationShip,
                                 ImageUrl = us.ImageUrl,
                                 PhoneNumber = cz.PhoneNumber
                             });

                var result = await query.ToListAsync();
                var data = DataResult.ResultSuccess(result, "Get success!");
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
        public async Task<DataResult> AddMemberToFamily(AddMemeberInput input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var member = _memberRepos.FirstOrDefault(x => x.UserId == input.UserId && x.ApartmentCode == input.ApartmentCode);
                if (member != null) return DataResult.ResultFail("Thành viên đã tồn tại");

                var checkAdmin = _memberRepos.FirstOrDefault(x => x.UserId == AbpSession.UserId && x.ApartmentCode == input.ApartmentCode && x.IsAdmin == true);
                if (checkAdmin == null) return DataResult.ResultFail("Bạn không có quyền thêm thành viên");
                var memberAdd = new HomeMember()
                {
                    ApartmentCode = input.ApartmentCode,
                    UserId = input.UserId,
                    TenantId = AbpSession.TenantId,
                    IsActive = true
                };
                await _memberRepos.InsertAsync(memberAdd);

                var data = DataResult.ResultSuccess("Get success!");
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
        public async Task<object> CreateOrUpdateCitizen(CreateOrUpdateInput input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                if (!string.IsNullOrEmpty(input.FullName))
                {
                    var checkCitizen = await _citizenRepos.FirstOrDefaultAsync(x => x.ApartmentCode == input.ApartmentCode && x.AccountId != AbpSession.UserId
                    && !string.IsNullOrEmpty(x.FullName) && x.FullName.Trim().ToLower() == input.FullName.Trim().ToLower() && x.BirthYear == input.BirthYear
                    && (x.State != STATE_CITIZEN.ACCEPTED || x.State != STATE_CITIZEN.NEW));
                    if (checkCitizen != null) throw new UserFriendlyException(1, "Cư dân đã tồn tại");
                }
                var updateData = await _citizenRepos.FirstOrDefaultAsync(x => x.AccountId == AbpSession.UserId);
                if (updateData != null && updateData.State == STATE_CITIZEN.ACCEPTED)
                {
                    await UpdateCitizenInfo(input, updateData);
                    mb.statisticMetris(t1, 0, "Ud_citizen");
                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                input.TenantId = AbpSession.TenantId;
                var isVoter = (input.RelationShip == RELATIONSHIP.Contractor) ? true : false;

                //check thong tin dang ky voi thong tin cu dan tren he thong: AparmentCode, FullName, BirthYear
                input.State = await HanleCheckCitizenInternal(input);

                // kiem tra cu dan thuoc can ho nao ?
                if (!string.IsNullOrWhiteSpace(input.ApartmentCode))
                {
                    await CheckCitizenApartmentCode(input.ApartmentCode, AbpSession.UserId.Value, isVoter);
                }

                if (updateData != null)
                {
                    await VerifyAgainCitizen(input, updateData);
                    mb.statisticMetris(t1, 0, "Ud_citizen");
                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {
                    var insertInput = input.MapTo<Citizen>();
                    insertInput.AccountId = AbpSession.UserId;
                    long id = await _citizenRepos.InsertAndGetIdAsync(insertInput);
                    insertInput.Id = id;
                    var admins = await _store.GetAllCitizenManagerTenantAsync(AbpSession.TenantId);

                    if (admins != null && admins.Any())
                    {
                        await NotifierVerifyCitizen(insertInput, insertInput.FullName, admins.ToArray());
                    }

                    mb.statisticMetris(t1, 0, "is_citizen");
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
        public async Task<object> CreateCitizen(CreateCitizenByUserInput input)
        {
            try
            {
                Citizen? citizen = await _citizenRepos.FirstOrDefaultAsync(x => (x.ApartmentCode == input.ApartmentCode
                    && x.FullName.ToLower().Trim() == input.FullName.ToLower().Trim()));
                if (citizen != null)
                {
                    throw new UserFriendlyException(409, "Cư dân đã tồn tại");
                }
                Citizen citizenInsert = input.MapTo<Citizen>();
                citizenInsert.TenantId = AbpSession.TenantId;
                citizenInsert.AccountId = AbpSession.UserId;
                citizenInsert.State = STATE_CITIZEN.NEW;
                await _citizenRepos.InsertAndGetIdAsync(citizenInsert);
                var admins = await _store.GetAllCitizenManagerTenantAsync(AbpSession.TenantId);

                if (admins != null && admins.Any())
                {
                    await NotifierVerifyCitizen(citizenInsert, citizenInsert.FullName, admins.ToArray());
                }


                return DataResult.ResultSuccess(true, "Insert success !");
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<object> UpdateCitizen(UpdateCitizenByUserInput input)
        {
            try
            {
                Citizen? citizenUpdate = await _citizenRepos.FirstOrDefaultAsync(x => x.Id == input.Id)
                    ?? throw new UserFriendlyException("Cư dân không tồn tại");
                citizenUpdate.FullName = input.FullName;
                citizenUpdate.Address = input.Address;
                citizenUpdate.Nationality = input.Nationality;
                citizenUpdate.IdentityNumber = input.IdentityNumber;
                citizenUpdate.PhoneNumber = input.PhoneNumber;
                citizenUpdate.Email = input.Email;
                citizenUpdate.Gender = input.Gender;
                citizenUpdate.DateOfBirth = input.DateOfBirth;
                citizenUpdate.ApartmentCode = input.ApartmentCode;
                citizenUpdate.BuildingCode = input.BuildingCode;
                citizenUpdate.OtherPhones = input.OtherPhones;
                citizenUpdate.BirthYear = input.BirthYear;
                citizenUpdate.OrganizationUnitId = input.OrganizationUnitId;
                citizenUpdate.UrbanCode = input.UrbanCode;
                citizenUpdate.RelationShip = input.RelationShip;
                citizenUpdate.MemberNum = input.MemberNum;
                citizenUpdate.BuildingId = input.BuildingId;
                citizenUpdate.UrbanId = input.UrbanId;
                citizenUpdate.IdentityImageUrls = input.IdentityImageUrls;
                citizenUpdate.State = STATE_CITIZEN.NEW;
                citizenUpdate.HomeAddress = input.HomeAddress;

                await _citizenRepos.UpdateAsync(citizenUpdate);
                return DataResult.ResultSuccess(true, "Update success !");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        private async Task UpdateCitizenInfo(CreateOrUpdateInput input, Citizen updateData)
        {
            input.Id = updateData.Id;
            input.State = updateData.State;
            input.FullName = updateData.FullName;
            input.ApartmentCode = updateData.ApartmentCode;
            input.AccountId = AbpSession.UserId;
            input.MapTo(updateData);
            await _citizenRepos.UpdateAsync(updateData);
        }
        private async Task VerifyAgainCitizen(CreateOrUpdateInput input, Citizen updateData)
        {
            input.Id = updateData.Id;
            input.AccountId = AbpSession.UserId;
            input.MapTo(updateData);
            await _citizenRepos.UpdateAsync(updateData);
        }
        private async Task<STATE_CITIZEN> HanleCheckCitizenInternal(CreateOrUpdateInput input)
        {
            try
            {
                if (input.FullName != null)
                {
                    var query = (from ctemp in _citizenTempRepos.GetAll()
                                 where ctemp.ApartmentCode.Trim() == input.ApartmentCode.Trim()
                                 && ctemp.FullName.Trim().ToLower() == input.FullName.Trim().ToLower()
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
                                     OrganizationUnitId = ctemp.OrganizationUnitId

                                 }).AsQueryable();
                    if (query.Count() > 0)
                    {

                        var verify = new CitizenVerification();
                        verify.BuildingCode = input.BuildingCode;
                        verify.MatchCount = 2;
                        verify.TenantId = AbpSession.TenantId;
                        if (input.PhoneNumber != null)
                        {
                            var querytmp = query.Where(x => x.PhoneNumber.Trim().ToLower() == input.PhoneNumber.Trim().ToLower());
                            if (querytmp.Count() > 0)
                            {
                                verify.MatchCount++;
                                query = querytmp;
                            }
                            else
                            {
                                verify.PhoneNumber = input.PhoneNumber;
                            }
                        }
                        if (input.Nationality != null)
                        {
                            var querytmp = query.Where(x => x.Nationality.Trim().ToLower() == input.Nationality.Trim().ToLower());
                            if (querytmp.Count() > 0)
                            {
                                verify.MatchCount++;
                                query = querytmp;
                            }
                            else
                            {
                                verify.Nationality = input.Nationality;
                            }
                        }
                        if (input.Email != null)
                        {
                            var querytmp = query.Where(x => x.Email.Trim().ToLower() == input.Email.Trim().ToLower());
                            if (querytmp.Count() > 0)
                            {
                                verify.MatchCount++;
                                query = querytmp;
                            }
                            else
                            {
                                verify.Email = input.Email;
                            }
                        }
                        if (input.Gender != null)
                        {
                            var querytmp = query.Where(x => x.Gender.Trim().ToLower() == input.Gender.Trim().ToLower());
                            if (querytmp.Count() > 0)
                            {
                                verify.MatchCount++;
                                query = querytmp;
                            }
                            else
                            {
                                verify.Gender = input.Gender;
                            }
                        }
                        if (input.BirthYear != null)
                        {
                            var querytmp = query.Where(x => x.BirthYear == input.BirthYear);
                            if (querytmp.Count() > 0)
                            {
                                verify.MatchCount++;
                                query = querytmp;
                            }
                            else
                            {
                                verify.BirthYear = input.BirthYear;
                            }
                        }
                        if (input.RelationShip != null)
                        {
                            var querytmp = query.Where(x => x.RelationShip == input.RelationShip);
                            if (querytmp.Count() > 0)
                            {
                                verify.MatchCount++;
                                query = querytmp;
                            }
                            else
                            {
                                verify.RelationShip = input.RelationShip;
                            }
                        }

                        if (input.Address != null)
                        {
                            var querytmp = query.Where(x => x.Address == input.Address);
                            if (querytmp.Count() > 0)
                            {
                                verify.MatchCount++;
                                query = querytmp;
                            }
                            else
                            {
                                verify.Address = input.Address;
                            }
                        }

                        verify.CitizenTempId = query.First().Id;
                        input.CitizenTempId = verify.CitizenTempId;
                        if (verify.MatchCount > 2)
                        {
                            return STATE_CITIZEN.ACCEPTED;
                            // In.State = STATE_CITIZEN.ACCEPTED;
                        }
                        await _citizenVerificationRepos.InsertAsync(verify);
                    }


                }
                return STATE_CITIZEN.NEW;
            }
            catch (Exception)
            {
                return STATE_CITIZEN.NEW;
            }
        }
        public async Task<DataResult> GetInformationOfCitizen()
        {
            try
            {
                IQueryable<Citizen> query = _citizenRepos.GetAll().Where(x => x.AccountId == AbpSession.UserId);
                Citizen citizen = query.FirstOrDefault(x => x.State == STATE_CITIZEN.ACCEPTED) ?? query.FirstOrDefault();

                List<string> apartmentCodes = query
                    .Where(x => x.State == STATE_CITIZEN.ACCEPTED || citizen.State == STATE_CITIZEN.ACCEPTED)
                    .Select(x => x.ApartmentCode)
                    .ToList();

                List<ApartmentInfoDto> apartmentInfoDtos = _apartmentRepository.GetAll()
                    .Where(x => apartmentCodes.Contains(x.ApartmentCode))
                    .Select(apartment => new ApartmentInfoDto
                    {
                        Id = apartment.Id,
                        ApartmentCode = apartment.ApartmentCode,
                        Name = apartment.Name,
                    })
                    .ToList();
                return DataResult.ResultSuccess(new CitizenInfoDto()
                {
                    ApartmentCodes = apartmentInfoDtos,
                    CitizenInfo = citizen
                }, "Get success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> GetCitizenInfo()
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var result = await _citizenRepos.FirstOrDefaultAsync(x => x.AccountId == AbpSession.UserId);
                var data = DataResult.ResultSuccess(result, "Get success!");
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

        #region Vote

        public async Task<object> CreateOrUpdateUserVote(UserVoteDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var check = await _userVoteRepos.FirstOrDefaultAsync(x => x.CityVoteId == input.CityVoteId && x.CreatorUserId == AbpSession.UserId);

                input.TenantId = AbpSession.TenantId;
                if (input.Id > 0 || check != null)
                {
                    //update
                    var updateData = await _userVoteRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);

                        //call back
                        await _userVoteRepos.UpdateAsync(updateData);
                    }
                    else
                    {
                        input.MapTo(check);

                        //call back
                        await _userVoteRepos.UpdateAsync(updateData);
                    }
                    mb.statisticMetris(t1, 0, "Ud_uservote");

                    var data = DataResult.ResultSuccess(input, "Update success !");
                    return data;
                }
                else if (check == null)
                {

                    var insertInput = input.MapTo<UserVote>();
                    long id = await _userVoteRepos.InsertAndGetIdAsync(insertInput);
                    insertInput.Id = id;
                    mb.statisticMetris(t1, 0, "is_uservote");
                    var data = DataResult.ResultSuccess(insertInput, "Insert success !");
                    return data;
                }
                return DataResult.ResultFail("User is voted !"); ;

            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> GetAllCityVotes(GetAllCityVoteInput input)
        {
            try
            {
                var totalUsers = _userRepos.GetAll().Count();
                totalUsers = totalUsers > 0 ? totalUsers : 1;
                var query = (from cv in _cityVoteRepos.GetAll()
                             join ou in _organizationUnitRepository.GetAll() on cv.OrganizationUnitId equals ou.Id into tb_ou
                             from ou in tb_ou.DefaultIfEmpty()
                                 //join sv in _userVoteRepos.GetAll() on cv.Id equals sv.CityVoteId into tb_sv
                                 //from ci in tb_sv.DefaultIfEmpty()
                             select new CityVoteDto()
                             {
                                 CreatorUserId = cv.CreatorUserId,
                                 CreationTime = cv.CreationTime,
                                 Options = cv.Options,
                                 Name = cv.Name,
                                 OrganizationUnitId = cv.OrganizationUnitId,
                                 Id = cv.Id,
                                 StartTime = cv.StartTime,
                                 FinishTime = cv.FinishTime,
                                 TenantId = cv.TenantId,
                                 LastModificationTime = cv.LastModificationTime,
                                 LastModifierUserId = cv.LastModifierUserId,
                                 TotalVotes = (from sv in _userVoteRepos.GetAll()
                                               where sv.CityVoteId == cv.Id
                                               select sv).Count(),
                                 UserVotes = (from sv in _userVoteRepos.GetAll()
                                              where sv.CityVoteId == cv.Id
                                              select sv).ToList(),
                                 TotalUsers = totalUsers,
                                 OrganizationName = ou.DisplayName
                             })
                             .WhereIf(input.OrganizationUnitId.HasValue, x => x.OrganizationUnitId == input.OrganizationUnitId)
                             .AsQueryable();
                var now = DateTime.Now;
                switch (input.State)
                {
                    //new
                    case STATE_GET_VOTE.NEW:
                        query = query.Where(x => x.FinishTime >= DateTime.Now && x.UserVotes.FirstOrDefault(u => u.CreatorUserId == AbpSession.UserId) == null);
                        break;
                    // voted
                    case STATE_GET_VOTE.NEWVOTED:
                        query = query.Where(x => x.FinishTime >= DateTime.Now && x.UserVotes.FirstOrDefault(u => u.CreatorUserId == AbpSession.UserId) != null);
                        break;
                    case STATE_GET_VOTE.FINISH:
                        query = query.Where(x => x.FinishTime < DateTime.Now);
                        break;
                    default:
                        break;
                }

                var dataGrids = query.PageBy(input).ToList();
                if (dataGrids != null)
                {
                    foreach (var item in dataGrids)
                    {
                        try
                        {
                            item.VoteOptions = JsonConvert.DeserializeObject<List<VoteOption>>(item.Options);
                        }
                        catch (Exception ex)
                        {

                        }
                        if (item.VoteOptions != null)
                        {
                            foreach (var opt in item.VoteOptions)
                            {
                                opt.CountVote = await _userVoteRepos.GetAll().Where(x => x.CityVoteId == item.Id && x.OptionVoteId == opt.Id).CountAsync();
                                opt.Percent = totalUsers > 0 ? (float)Math.Round((float)((float)opt.CountVote / (float)totalUsers), 3) : 0;

                            }
                        }
                        if (!string.IsNullOrWhiteSpace(item.OrganizationName))
                        {
                            item.Name = $"( {item.OrganizationName} ) " + item.Name;
                        }
                    }
                }
                var data = DataResult.ResultSuccess(dataGrids, "Get success!", query.Count());
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

        #region common

        private async Task CheckCitizenApartmentCode(string apartmentCode, long userId, bool isVoter)
        {

            try
            {
                var smarthome = await _smartHomeRepos.FirstOrDefaultAsync(x => x.ApartmentCode == apartmentCode);
                if (smarthome == null) return;

                var member = _memberRepos.FirstOrDefault(x => x.UserId == userId && x.SmartHomeCode == smarthome.SmartHomeCode);
                if (member != null) return;

                member = new HomeMember()
                {
                    SmartHomeCode = smarthome.SmartHomeCode,
                    IsAdmin = isVoter ? true : false,
                    UserId = userId,
                    TenantId = AbpSession.TenantId,
                    IsVoter = isVoter,
                    ApartmentCode = apartmentCode,
                    IsActive = false
                };
                _memberRepos.Insert(member);

            }
            catch
            {

            }
        }

        public async Task<object> GetUserCitizenTempByCitizenId(long id)
        {
            try
            {
                var citizenTempId = (await _citizenRepos.GetAsync(id)).CitizenTempId;
                if (citizenTempId.HasValue)
                {
                    var query = await _citizenTempRepos.GetAsync(citizenTempId.Value);
                    return DataResult.ResultSuccess(query, "Get success!");
                }
                else throw new UserFriendlyException("CitizenTemp not found!");

            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<object> GetFamilyInfoByCitizenId(GetFamilyInfoByApartmentCode input)
        {
            Console.WriteLine(input.ApartmentCode);
            try
            {
                var query = (from citizen in _citizenTempRepos.GetAll()
                                 //where citizen.ApartmentCode.Trim().ToUpper().Contains(input.ApartmentCode.Trim().ToUpper())
                             where citizen.ApartmentCode.Trim().ToUpper() == input.ApartmentCode.Trim().ToUpper()
                             select new CitizenRole
                             {
                                 Name = citizen.FullName,
                                 Type = citizen.Type,
                                 RelationShip = citizen.RelationShip,
                                 Nationality = citizen.Nationality,
                                 Contact = citizen.PhoneNumber,
                                 OwnerGeneration = citizen.OwnerGeneration > 0 ? citizen.OwnerGeneration : 0,
                                 OwnerId = citizen.OwnerId
                             }).AsQueryable();
                var result = await query.PageBy(input).ToListAsync();
                return DataResult.ResultSuccess(result, "Get success!", query.Count());
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        private async Task NotifierVerifyCitizen(Citizen citizen, string citizenName, UserIdentifier[] admin)
        {
            var detailUrlApp = $"yooioc://app/verify-citizen/detail?id={citizen.Id}";
            var detailUrlWA = $"/citizens/verify/id={citizen.Id}";
            var messageDeclined = new UserMessageNotificationDataBase(
            AppNotificationAction.CitizenVerify,
            AppNotificationIcon.CitizenVerifyIcon,
            TypeAction.Detail,
            $"{citizenName} đã gửi yêu cầu xác minh tài khoản đến ban quản trị. Nhấn để xem chi tiết !",
            detailUrlApp,
            detailUrlWA);
            await _appNotifier.SendMessageNotificationInternalAsync(
                "Thông báo xác minh tài khoản cư dân!",
                 $"{citizenName} đã gửi yêu cầu xác minh tài khoản đến ban quản trị. Nhấn để xem chi tiết !",
                 detailUrlApp,
                 detailUrlWA,
                admin.ToArray(),
                messageDeclined,
                AppType.IOC);

        }
        #endregion
    }
}
