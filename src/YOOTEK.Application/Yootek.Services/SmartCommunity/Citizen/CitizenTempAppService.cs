using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Notifications;
using Abp.UI;
using Yootek.Application;
using Yootek.AppManager.HomeMembers;
using Yootek.Authorization;
using Yootek.Authorization.Users;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Yootek.Organizations;
using Yootek.QueriesExtension;
using Yootek.Services.Dto;
using Yootek.Users.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace Yootek.Services
{
    public interface ICitizenTempAppService : IApplicationService
    {
        Task<object> GetAllCitizenAsync(GetAllCitizenInput input);
        Task<object> GetCitizenByIdAsync(long id);
        Task<object> CreateOrUpdateCitizen(UpdateOrCreateCitizenTemp input);
        Task<object> DeleteCitizen(long id);
        Task<object> ExportCitizenTempToExcel(ExportCitizenTempDto input);
        Task<DataResult> GetStaticCitizenAsync(GetStatisticCitizenInput input);
        Task<object> GetCitizenInfoByAccountId(CitizenVerificationdto citizenVerification);
    }

    //[AbpAuthorize(PermissionNames.Pages_Citizens_List, PermissionNames.Pages_Residents_Information)]
    [AbpAuthorize]
    public class CitizenTempAppService : YootekAppServiceBase, ICitizenTempAppService
    {
        private readonly IRepository<CitizenTemp, long> _citizenTempRepos;
        private readonly IRepository<HomeMember, long> _homeMemberRepos;
        private readonly IRepository<User, long> _userRepos;
        private readonly IRepository<AppOrganizationUnit, long> _organizationUnitRepository;
        private readonly IRepository<UserOrganizationUnit, long> _userOrganizationUnitRepository;
        private readonly IRepository<CareerCategory, long> _careerCategoryRepository;
        private readonly IRepository<Apartment, long> _apartmentRepos;
        private readonly ICitizenTempExcelExporter _citizenTempExcelExporter;
        private readonly IRepository<Citizen, long> _citizenRepos;
        private readonly IHomeMemberManager _homeMemberManager;
        private readonly INotificationSubscriptionManager _notificationSubscriptionManager;
        private readonly UserManager _userManager;

        public CitizenTempAppService(
          IRepository<User, long> userRepos,
          IRepository<CitizenTemp, long> citizenTempRepos,
          IRepository<AppOrganizationUnit, long> organizationUnitRepository,
          IRepository<UserOrganizationUnit, long> userOrganizationUnitRepository,
          IRepository<Apartment, long> apartmentRepos,
          IRepository<HomeMember, long> homeMemberRepos,
          ICitizenTempExcelExporter citizenTempExcelExporter,
          IRepository<Citizen, long> citizenRepos,
          IHomeMemberManager homeMemberManager,
          INotificationSubscriptionManager notificationSubscriptionManager,
          IRepository<CareerCategory, long> careerCategoryRepository,
          UserManager userManager
            )
        {
            _userRepos = userRepos;
            _citizenTempRepos = citizenTempRepos;
            _organizationUnitRepository = organizationUnitRepository;
            _userOrganizationUnitRepository = userOrganizationUnitRepository;
            _apartmentRepos = apartmentRepos;
            _citizenTempExcelExporter = citizenTempExcelExporter;
            _homeMemberRepos = homeMemberRepos;
            _citizenRepos = citizenRepos;
            _homeMemberManager = homeMemberManager;
            _userManager = userManager;
            _notificationSubscriptionManager = notificationSubscriptionManager;
            _careerCategoryRepository = careerCategoryRepository;
        }

        #region Citizen
        public async Task<DataResult> GetAllOwnerApartmentAsync(GetAllOwnerApartmentInput input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var query = (from cz in _citizenTempRepos.GetAll()
                             join ap in _apartmentRepos.GetAll() on cz.ApartmentCode equals ap.ApartmentCode into tb_ap
                             from ap in tb_ap.DefaultIfEmpty()
                             select new OwnerApartmentDto()
                             {
                                 Id = cz.Id,
                                 ApartmentCode = cz.ApartmentCode,
                                 CitizenTempId = cz.Id,
                                 FullName = cz.FullName,
                                 RelationShip = cz.RelationShip,
                                 OrganizationUnitId = cz.OrganizationUnitId,
                                 TenantId = cz.TenantId,
                                 IsStayed = cz.IsStayed,
                                 BuildingId = ap.BuildingId,
                                 UrbanId = ap.UrbanId,
                                 OwnerGeneration = cz.OwnerGeneration,
                                 ApartmentAreas = ap != null ? ap.Area : 0
                             })
                              .Where(x => x.RelationShip == RELATIONSHIP.Contractor)
                              .WhereIf(input.OrganizationUnitId.HasValue, x => x.OrganizationUnitId == input.OrganizationUnitId)
                              .WhereIf(input.IsStayed.HasValue, x => x.IsStayed == input.IsStayed)
                              .WhereIf(!string.IsNullOrEmpty(input.ApartmentCode), x => x.ApartmentCode == input.ApartmentCode)
                              .AsQueryable();

                if (!input.IsAllMember)
                {
                    query = query.Where(x => x.RelationShip == RELATIONSHIP.Contractor);
                }

                var result = query.ToList();
                mb.statisticMetris(t1, 0, "is_citizen");
                var data = DataResult.ResultSuccess(result, "Insert success !", query.Count());
                return data;

            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> CreateOrUpdateCitizen(UpdateOrCreateCitizenTemp input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                input.TenantId = AbpSession.TenantId;
                if (input.Id > 0)
                {
                    //update
                    var updateData = await _citizenTempRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);
                        if (input.UrbanId != updateData.UrbanId && input.ApartmentCode != updateData.ApartmentCode && input.BuildingId != updateData.BuildingId)
                        {
                            var olderOwners = _citizenTempRepos.GetAll()
                            .Where(x => x.RelationShip == RELATIONSHIP.Contractor && x.ApartmentCode == updateData.ApartmentCode && x.Id != updateData.Id && x.IsStayed == updateData.IsStayed)
                            .OrderByDescending(x => x.OwnerGeneration)
                            .ToList();
                            var olderOwner = olderOwners.FirstOrDefault(); //chủ nhà có đời chủ cao nhất
                            if (updateData.RelationShip == RELATIONSHIP.Contractor) updateData.OwnerGeneration = (olderOwner.OwnerGeneration ?? 1) + 1;

                        }


                        //call back
                        await _citizenTempRepos.UpdateAsync(updateData);
                    }
                    mb.statisticMetris(t1, 0, "Ud_citizen");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {
                    var insertInput = input.MapTo<CitizenTemp>();
                    long id = await _citizenTempRepos.InsertAndGetIdAsync(insertInput);
                    insertInput.Id = id;

                    var olderOwners = _citizenTempRepos.GetAll()
                            .Where(x => x.RelationShip == RELATIONSHIP.Contractor && x.ApartmentCode == insertInput.ApartmentCode && x.Id != insertInput.Id && x.IsStayed == insertInput.IsStayed)
                            .OrderByDescending(x => x.OwnerGeneration)
                            .ToList();
                    var olderOwner = olderOwners.FirstOrDefault(); //chủ nhà có đời chủ cao nhất

                    if (insertInput.RelationShip == RELATIONSHIP.Contractor && olderOwner != null)
                    {
                        insertInput.OwnerGeneration = olderOwner.OwnerGeneration + 1;
                    }
                    else insertInput.OwnerGeneration = input.OwnerGeneration ?? olderOwner?.OwnerGeneration ?? 1;


                    //if (insertInput.RelationShip == RELATIONSHIP.Contractor)
                    //{
                    //    var olderOwners = _citizenTempRepos.GetAll()
                    //        .Where(x => x.RelationShip == RELATIONSHIP.Contractor && x.ApartmentCode == insertInput.ApartmentCode && x.Id != insertInput.Id)
                    //        .OrderByDescending(x => x.OwnerGeneration)
                    //        .ToList();
                    //    var olderOwner = olderOwners.FirstOrDefault(); //chủ nhà có đời chủ cao nhất
                    //    if (olderOwner == null) insertInput.OwnerGeneration = 1; //chưa có chủ nhà
                    //    //có chủ nhà
                    //    else
                    //    {
                    //        if (insertInput.IsStayed)
                    //        {
                    //            insertInput.OwnerGeneration = (olderOwner.OwnerGeneration ?? 1) + 1;

                    //        }
                    //        else
                    //        {
                    //            var ownerCurrent = _citizenTempRepos.GetAll()
                    //                .Where(x => x.RelationShip == RELATIONSHIP.Contractor && x.ApartmentCode == insertInput.ApartmentCode && x.IsStayed)
                    //                .OrderByDescending(x => x.OwnerGeneration)
                    //                .FirstOrDefault();

                    //            if (ownerCurrent == null || ownerCurrent.OwnerGeneration == 0)
                    //            {
                    //                insertInput.OwnerGeneration = 1;
                    //            }
                    //            else
                    //            {
                    //                insertInput.OwnerGeneration = ownerCurrent.OwnerGeneration;
                    //                ownerCurrent.OwnerGeneration = ownerCurrent.OwnerGeneration + 1;
                    //            }
                    //        }
                    //    }

                    //    var aptms = await _citizenTempRepos.GetAllListAsync(x => x.ApartmentCode == insertInput.ApartmentCode && x.Id != id && x.OwnerGeneration != insertInput.OwnerGeneration);
                    //    if (aptms != null)
                    //    {
                    //        foreach (var apt in aptms)
                    //        {
                    //            apt.IsStayed = false;
                    //            //apt.OwnerGeneration = insertInput.OwnerGeneration;
                    //        }
                    //    }
                    //}

                    await CurrentUnitOfWork.SaveChangesAsync();
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

        public async Task<DataResult> UpdateStayedCitizenTemp(CitizenTempDto input)
        {
            try
            {


                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    long t1 = TimeUtils.GetNanoseconds();
                    //update
                    var updateData = await _citizenTempRepos.GetAsync(input.Id);
                    var aptms = await _citizenTempRepos.GetAllListAsync(x => x.ApartmentCode == updateData.ApartmentCode && x.Id != input.Id && x.OwnerGeneration == updateData.OwnerGeneration && x.RelationShip != RELATIONSHIP.Contractor && x.IsStayed == updateData.IsStayed);

                    if (updateData.RelationShip == RELATIONSHIP.Contractor && aptms != null)
                    {
                        foreach (var apt in aptms)
                        {
                            apt.IsStayed = input.IsStayed;
                        }

                    }


                    if (updateData != null)
                    {
                        input.MapTo(updateData);
                        await _citizenTempRepos.UpdateAsync(updateData);

                    }


                    return DataResult.ResultSuccess(updateData, "Update citizen success !");
                }
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        private async Task CreateListCitizenAsync(List<CitizenTempDto> input)
        {
            try
            {
                if (input == null || !input.Any())
                {
                    return;
                }
                foreach (var ctzt in input)
                {
                    long id = await _citizenTempRepos.InsertAndGetIdAsync(ctzt);
                    ctzt.Id = id;
                    if (ctzt.RelationShip == RELATIONSHIP.Contractor)
                    {
                        var olderOwners = _citizenTempRepos.GetAll()
                            .Where(x => x.RelationShip == RELATIONSHIP.Contractor && x.ApartmentCode == ctzt.ApartmentCode && x.Id != id && x.IsStayed == ctzt.IsStayed)
                            .OrderByDescending(x => x.OwnerGeneration)
                            .ToList();
                        var olderOwner = olderOwners.FirstOrDefault();

                        if (olderOwner == null)
                        {
                            ctzt.OwnerGeneration = 1;
                        }
                        else
                        {
                            if (ctzt.RelationShip == RELATIONSHIP.Contractor)
                            {
                                ctzt.OwnerGeneration = (olderOwner.OwnerGeneration ?? 1) + 1;
                            }
                            else
                            {
                                var owner = _citizenTempRepos.FirstOrDefault(x => x.Id == ctzt.OwnerId);
                                ctzt.OwnerGeneration = ctzt.OwnerId != null
                                    ? owner?.OwnerGeneration ?? 1
                                    : 1;
                            }
                        }

                        //if (olderOwner == null)
                        //{
                        //    if (ctzt.IsStayed)
                        //    {
                        //        ctzt.OwnerGeneration = 1;
                        //    }
                        //    else
                        //    {
                        //        ctzt.OwnerGeneration = 0;
                        //    }
                        //}
                        //else
                        //{
                        //    if (ctzt.IsStayed)
                        //    {
                        //        ctzt.OwnerGeneration = olderOwner.OwnerGeneration + 1;
                        //    }
                        //    else
                        //    {
                        //        var ownerCurrent = _citizenTempRepos.GetAll()
                        //            .Where(x => x.RelationShip == RELATIONSHIP.Contractor && x.ApartmentCode == ctzt.ApartmentCode && x.IsStayed)
                        //            .OrderByDescending(x => x.OwnerGeneration)
                        //            .FirstOrDefault();

                        //        if (ownerCurrent == null || ownerCurrent.OwnerGeneration == 0)
                        //        {
                        //            ctzt.OwnerGeneration = 1;
                        //        }
                        //        else
                        //        {
                        //            ctzt.OwnerGeneration = ownerCurrent.OwnerGeneration;
                        //            ownerCurrent.OwnerGeneration = ownerCurrent.OwnerGeneration + 1;
                        //        }
                        //    }
                        //}

                        //var aptms = await _citizenTempRepos.GetAllListAsync(x => x.ApartmentCode == ctzt.ApartmentCode && x.Id != id && x.OwnerGeneration != ctzt.OwnerGeneration);
                        //if (aptms != null)
                        //{
                        //    foreach (var apt in aptms)
                        //    {
                        //        apt.IsStayed = false;                            }
                        //}
                    }
                }
                await CurrentUnitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Exception !");
                Logger.Fatal(ex.Message);
                throw;
            }
        }

        public async Task<object> DeleteCitizen(long id)
        {
            try
            {

                var citizenDelete = await _citizenTempRepos.GetAsync(id);
                if (citizenDelete.RelationShip == RELATIONSHIP.Contractor)
                {
                    var apartmentCodeDelete = citizenDelete.ApartmentCode;
                    var urbanIdDelete = citizenDelete.UrbanId;
                    var buildingIdDelete = citizenDelete.BuildingId;

                    // Kiểm tra xem có cư dân nào khác có quan hệ là wife hoặc husband hay không
                    var otherResidents = _citizenTempRepos.GetAll()
                        .Where(x => x.ApartmentCode == apartmentCodeDelete &&
                                    x.UrbanId == urbanIdDelete &&
                                    x.BuildingId == buildingIdDelete &&
                                    x.IsStayed == citizenDelete.IsStayed &&
                                    (x.RelationShip == RELATIONSHIP.Wife || x.RelationShip == RELATIONSHIP.Husband))
                        .ToList();

                    if (otherResidents.Count > 0)
                    {
                        otherResidents[0].RelationShip = RELATIONSHIP.Contractor;
                    }
                    else
                    {
                        // Xóa tất cả các cư dân khác
                        var residentsDelete = _citizenTempRepos.GetAll()
                            .Where(x => x.ApartmentCode == apartmentCodeDelete &&
                                        x.UrbanId == urbanIdDelete &&
                                        x.BuildingId == buildingIdDelete &&
                                        x.RelationShip != RELATIONSHIP.Contractor)
                            .ToList();

                        foreach (var resident in residentsDelete)
                        {
                            await _citizenTempRepos.DeleteAsync(resident.Id);
                        }

                        //// Kiểm tra xem còn cư dân nào khác
                        //var remainingResidents = _citizenTempRepos.GetAll()
                        //    .Where(x => x.ApartmentCode == apartmentCodeDelete &&
                        //                x.UrbanId == urbanIdDelete &&
                        //                x.BuildingId == buildingIdDelete &&
                        //                x.IsStayed == citizenDelete.IsStayed &&
                        //                x.RelationShip != RELATIONSHIP.Contractor)
                        //    .ToList();

                        //if (remainingResidents.Count == 0)
                        //{
                        //    // Giảm OwnerGeneration của tất cả các cư dân có OwnerGeneration lớn hơn OwnerGeneration
                        //    var ownerGenerationDelete = citizenDelete.OwnerGeneration;

                        //    var residentsHigherOwnerGeneration = _citizenTempRepos.GetAll()
                        //        .Where(x => x.ApartmentCode == apartmentCodeDelete &&
                        //                    x.UrbanId == urbanIdDelete &&
                        //                    x.BuildingId == buildingIdDelete &&
                        //                    x.OwnerGeneration > ownerGenerationDelete)
                        //        .ToList();

                        //    foreach (var resident in residentsHigherOwnerGeneration)
                        //    {
                        //        resident.OwnerGeneration -= 1;
                        //        await _citizenTempRepos.UpdateAsync(resident);
                        //    }
                        //}
                    }
                }

                // Xóa cư dân
                await _citizenTempRepos.DeleteAsync(id);

                var data = DataResult.ResultSuccess("Delete success!");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> GetStaticCitizenAsync(GetStatisticCitizenInput input)
        {
            Dictionary<string, int> dataResult = new Dictionary<string, int>();
            switch (input.QueryCase)
            {
                case QueryCaseCitizenStatistics.ByAgeAndSex:
                    using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                    {
                        List<long> buIds = UserManager.GetAccessibleBuildingOrUrbanIds();
                        var query = (from ci in _citizenTempRepos.GetAll()
                                     select new CitizenTempDto()
                                     {
                                         Id = ci.Id,
                                         Gender = ci.Gender,
                                         DateOfBirth = ci.DateOfBirth,
                                         BuildingId = ci.BuildingId,
                                         UrbanId = ci.UrbanId,
                                     })
                                .WhereByBuildingOrUrbanIf(!IsGranted(IOCPermissionNames.Data_Admin), buIds)
                                .WhereIf(input.UrbanId.HasValue, x => x.UrbanId == input.UrbanId)
                                .WhereIf(input.BuildingId.HasValue, x => x.BuildingId == input.BuildingId)
                                ;

                        if (input.OrganizationUnitId.HasValue) query = query.WhereIf(input.OrganizationUnitId.HasValue, x => x.OrganizationUnitId == input.OrganizationUnitId);
                        if (input.Sex.HasValue)
                            query = query.WhereIf(input.Sex.HasValue, x => !String.IsNullOrEmpty(x.Gender) && x.Gender.Equals(input.Sex.Value == 1 ? "Nam" : "Nữ"));
                        var dataCount = await query.ToListAsync();
                        dataResult.Add("Dưới 18 tuổi", dataCount.Count(x => x.DateOfBirth.HasValue && (DateTime.Now.Year - x.DateOfBirth.Value.Year) < 18));
                        dataResult.Add("Từ 18 - 25 tuổi", dataCount.Count(x => x.DateOfBirth.HasValue && (DateTime.Now.Year - x.DateOfBirth.Value.Year) > 18 && (DateTime.Now.Year - x.DateOfBirth.Value.Year) <= 25));
                        dataResult.Add("Từ 26 - 30 tuổi", dataCount.Count(x => x.DateOfBirth.HasValue && (DateTime.Now.Year - x.DateOfBirth.Value.Year) > 25 && (DateTime.Now.Year - x.DateOfBirth.Value.Year) <= 30));
                        dataResult.Add("Từ 31 - 40 tuổi", dataCount.Count(x => x.DateOfBirth.HasValue && (DateTime.Now.Year - x.DateOfBirth.Value.Year) > 30 && (DateTime.Now.Year - x.DateOfBirth.Value.Year) <= 40));
                        dataResult.Add("Từ 41 - 50 tuổi", dataCount.Count(x => x.DateOfBirth.HasValue && (DateTime.Now.Year - x.DateOfBirth.Value.Year) > 40 && (DateTime.Now.Year - x.DateOfBirth.Value.Year) <= 50));
                        dataResult.Add("Trên 50 tuổi", dataCount.Count(x => x.DateOfBirth.HasValue && (DateTime.Now.Year - x.DateOfBirth.Value.Year) > 50));
                    }
                    break;
                case QueryCaseCitizenStatistics.ByCareer:
                    using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                    {
                        List<long> buIds = UserManager.GetAccessibleBuildingOrUrbanIds();
                        var query = (from ci in _citizenTempRepos.GetAll()
                                     select new CitizenTempDto()
                                     {
                                         Id = ci.Id,
                                         CareerCategoryId = ci.CareerCategoryId,
                                         BuildingId = ci.BuildingId,
                                         UrbanId = ci.UrbanId,
                                     })
                                .WhereByBuildingOrUrbanIf(!IsGranted(IOCPermissionNames.Data_Admin), buIds)
                                .WhereIf(input.UrbanId.HasValue, x => x.UrbanId == input.UrbanId)
                                .WhereIf(input.BuildingId.HasValue, x => x.BuildingId == input.BuildingId)
                                ;

                        if (input.OrganizationUnitId.HasValue) query = query.WhereIf(input.OrganizationUnitId.HasValue, x => x.OrganizationUnitId == input.OrganizationUnitId);

                        var dataCount = await query.ToListAsync();
                        IQueryable<CareerCategoryDto> queryCareer = (from o in _careerCategoryRepository.GetAll()
                                                                     select new CareerCategoryDto
                                                                     {
                                                                         Id = o.Id,
                                                                         Title = o.Title,
                                                                         Code = o.Code,
                                                                     });
                        var dataCareer = await queryCareer.ToListAsync();
                        foreach (var item in dataCareer)
                        {
                            dataResult.Add(item.Title, dataCount.Count(x => x.CareerCategoryId.HasValue && x.CareerCategoryId.Value == item.Id));
                        }
                        dataResult.Add("Khác", dataCount.Count(x => (x.CareerCategoryId.HasValue && x.CareerCategoryId.Value == 0) || !x.CareerCategoryId.HasValue));
                    }
                    break;
            }
            var data = DataResult.ResultSuccess(dataResult, "Get success!");
            return data;
        }
        public async Task<object> GetAllCitizenAsync(GetAllCitizenInput input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    List<long> buIds = UserManager.GetAccessibleBuildingOrUrbanIds();
                    var query = (from ci in _citizenTempRepos.GetAll()
                                 join ct in _citizenRepos.GetAll() on ci.Id equals ct.CitizenTempId into tbl_citizen
                                 from ct in tbl_citizen.DefaultIfEmpty()
                                 join user in _userRepos.GetAll() on ci.AccountId equals user.Id into tbl_users
                                 from user in tbl_users.DefaultIfEmpty()
                                 select new CitizenTempDto()
                                 {
                                     Id = ci.Id,
                                     FullName = ci.FullName,
                                     Address = ci.Address,
                                     Nationality = ci.Nationality,
                                     IdentityNumber = ci.IdentityNumber,
                                     TenantId = ci.TenantId,
                                     ImageUrl = ci.ImageUrl,
                                     PhoneNumber = ci.PhoneNumber,
                                     //Email = ci.Email.Contains("yootek") ? ci.Email : null,
                                     Email = ci.Email,
                                     Gender = ci.Gender,
                                     DateOfBirth = ci.DateOfBirth,
                                     IsVoter = ci.IsVoter,
                                     ApartmentCode = ci.ApartmentCode,
                                     State = ci.State,
                                     BuildingCode = ci.BuildingCode,
                                     Type = ci.Type,
                                     IsStayed = ci.IsStayed,
                                     OtherPhones = ci.OtherPhones,
                                     BirthYear = ci.BirthYear,
                                     OrganizationUnitId = ci.OrganizationUnitId,
                                     UrbanCode = ci.UrbanCode,
                                     UrbanId = ci.UrbanId,
                                     BuildingId = ci.BuildingId,
                                     CitizenCode = ci.CitizenCode,
                                     RelationShip = ci.RelationShip,
                                     MemberNum = ci.MemberNum,
                                     CareerCategoryId = ci.CareerCategoryId,
                                     Career = ci.Career,
                                     OwnerGeneration = ci.OwnerGeneration,
                                     OwnerId = ci.OwnerId,
                                     TaxCode = ci.TaxCode,
                                     UserName = user.UserName,
                                     FirstName = user.Name,
                                     Surname = user.Surname,
                                     AccountId = user.Id,
                                     AccountDOB = user.DateOfBirth,
                                     AccountEmail = user.EmailAddress,
                                     Hometown = ci.Hometown,
                                 })
                            .WhereByBuildingOrUrbanIf(!IsGranted(IOCPermissionNames.Data_Admin), buIds)
                            .WhereIf(input.IsStayed.HasValue, x => x.IsStayed == input.IsStayed)
                            .ApplySearchFilter(input.Keyword, x => x.FullName, x => x.Address, x => x.Email, x => x.PhoneNumber, x => x.ApartmentCode);

                    if (input.FormId == 5)
                    {
                        var qCitizen = _citizenRepos.GetAll();
                        query = query.Where(x => (qCitizen.FirstOrDefault(y => y.CitizenTempId == x.Id)) != null);
                    }

                    query = query.WhereIf(input.IsStayed.HasValue, x => x.IsStayed == input.IsStayed);

                    DateTime fromDay = new DateTime(), toDay = new DateTime();
                    DateTime dateTimeNow = DateTime.Now;

                    if (input.FromDay.HasValue)
                    {
                        fromDay = new DateTime(input.FromDay.Value.Year, input.FromDay.Value.Month, input.FromDay.Value.Day, 0, 0, 0);
                        query = query.WhereIf(input.FromDay.HasValue, u => (u.LastModificationTime.HasValue && u.LastModificationTime >= fromDay) || (!u.LastModificationTime.HasValue && u.CreationTime >= fromDay));
                    }
                    if (input.ToDay.HasValue)
                    {
                        toDay = new DateTime(input.ToDay.Value.Year, input.ToDay.Value.Month, input.ToDay.Value.Day, 23, 59, 59);
                        query = query.WhereIf(input.ToDay.HasValue, u => (u.LastModificationTime.HasValue && u.LastModificationTime <= toDay) || (!u.LastModificationTime.HasValue && u.CreationTime <= toDay));
                    }

                    if (input.State.HasValue)
                    {
                        query = query.WhereIf(input.State.HasValue, x => x.State == (int)input.State);
                    }
                    if (input.OrganizationUnitId != null) query = query.WhereIf(input.OrganizationUnitId.HasValue, x => x.OrganizationUnitId == input.OrganizationUnitId);
                    if (input.BuildingId.HasValue) query = query.WhereIf(input.BuildingId.HasValue, x => x.BuildingId == input.BuildingId);
                    if (input.UrbanId.HasValue) query = query.WhereIf(input.UrbanId.HasValue, x => x.UrbanId == input.UrbanId);
                    if (input.ApartmentCode != null) query = query.Where(u => u.ApartmentCode.ToUpper() == input.ApartmentCode.ToUpper());

                    if (input.RelationShip.HasValue) query = query.WhereIf(input.RelationShip.HasValue, u => u.RelationShip == input.RelationShip);
                    if (input.FromAge.HasValue) query = query.WhereIf(input.FromAge.HasValue, u => u.DateOfBirth.HasValue && u.DateOfBirth.Value.Year <= (dateTimeNow.Year - input.FromAge));
                    if (input.ToAge.HasValue) query = query.WhereIf(input.ToAge.HasValue, u => u.DateOfBirth.HasValue && u.DateOfBirth.Value.Year >= (dateTimeNow.Year - input.ToAge));


                    var result = await query
                        .ApplySort(input.OrderBy, input.SortBy)
                        .ApplySort(OrderByCitizen.APARTMENT_CODE)  // sort default
                        .PageBy(input).ToListAsync();

                    if (result != null && result.Count() > 0)
                    {
                        foreach (var item in result)
                        {
                            var urbanDf = _organizationUnitRepository.FirstOrDefault(x => x.Type == APP_ORGANIZATION_TYPE.URBAN);
                            if (item.UrbanId.HasValue && urbanDf != null)
                                if (item.UrbanId.HasValue)
                                {
                                    var urban = _organizationUnitRepository.FirstOrDefault(x => x.Id == item.UrbanId);
                                    item.UrbanName = urban != null ? urban.DisplayName : urbanDf.DisplayName;
                                }
                                else item.UrbanName = urbanDf.DisplayName;

                        }
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

        public async Task<object> GetCitizenByIdAsync(long id)
        {
            try
            {
                var result = await _citizenTempRepos.GetAsync(id);
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
        public async Task<object> GetCitizenInfoByAccountId(CitizenVerificationdto citizenVerification)
        {
            try
            {
                long startTime = TimeUtils.GetNanoseconds();

                IQueryable<CitizenTempVerificationDto> query =
                    from citizenTemp in _citizenTempRepos.GetAll()
                    select new CitizenTempVerificationDto
                    {
                        FullName = citizenTemp.FullName,
                        Address = citizenTemp.Address,
                        Nationality = citizenTemp.Nationality,
                        IdentityNumber = citizenTemp.IdentityNumber,
                        TenantId = citizenTemp.TenantId,
                        ImageUrl = citizenTemp.ImageUrl,
                        PhoneNumber = citizenTemp.PhoneNumber,
                        Email = citizenTemp.Email,
                        Gender = citizenTemp.Gender,
                        DateOfBirth = citizenTemp.DateOfBirth,
                        IsVoter = citizenTemp.IsVoter,
                        ApartmentCode = citizenTemp.ApartmentCode,
                        State = citizenTemp.State,
                        BuildingCode = citizenTemp.BuildingCode,
                        Type = citizenTemp.Type,
                        IsStayed = citizenTemp.IsStayed,
                        OtherPhones = citizenTemp.OtherPhones,
                        BirthYear = citizenTemp.BirthYear,
                        OrganizationUnitId = citizenTemp.OrganizationUnitId,
                        UrbanCode = citizenTemp.UrbanCode,
                        UrbanId = citizenTemp.UrbanId,
                        BuildingId = citizenTemp.BuildingId,
                        TaxCode = citizenTemp.TaxCode,
                        CitizenCode = citizenTemp.CitizenCode,
                        RelationShip = citizenTemp.RelationShip,
                        MemberNum = citizenTemp.MemberNum,
                        Career = citizenTemp.Career,
                        OwnerGeneration = citizenTemp.OwnerGeneration,
                        OwnerId = citizenTemp.OwnerId,
                        AccountId = citizenTemp.AccountId,
                        CareerCategoryId = citizenTemp.CareerCategoryId,
                        BuildingName = _organizationUnitRepository.GetAll().Where(x => x.Id == citizenTemp.BuildingId).Select(x => x.DisplayName).FirstOrDefault(),
                        Hometown = citizenTemp.Hometown
                    }
;

                query = query
                    .Where(x =>
                        x.ApartmentCode == citizenVerification.ApartmentCode &&
                        x.UrbanId == citizenVerification.UrbanId &&
                        x.BuildingId == citizenVerification.BuildingId
                    )
                    .WhereIf(!string.IsNullOrEmpty(citizenVerification.FullName), x => x.FullName == citizenVerification.FullName)
                    .WhereIf(citizenVerification.DateOfBirth.HasValue, x => x.DateOfBirth.Value.Date == citizenVerification.DateOfBirth);

                var result = await query.ToListAsync();

                var data = DataResult.ResultSuccess(result, "Get success!");
                mb.statisticMetris(startTime, 0, "get_info_citizen");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<object> DeleteMultipleCitizens([FromBody] List<long> ids)
        {
            try
            {
                if (ids.Count == 0) return DataResult.ResultError("Error", "Empty input!");
                await _citizenTempRepos.DeleteAsync(x => ids.Contains(x.Id));
                var data = DataResult.ResultSuccess("Deleted successfully!");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }


        public async Task<object> ExportCitizenTempToExcel(ExportCitizenTempDto input)
        {
            try
            {
                List<long> buIds = UserManager.GetAccessibleBuildingOrUrbanIds();
                var citizens = await _citizenTempRepos.GetAll()
                    .WhereByBuildingOrUrbanIf(!IsGranted(IOCPermissionNames.Data_Admin), buIds)
                    .WhereIf(input.IsStayed.HasValue, x => x.IsStayed == input.IsStayed)
                    .WhereIf(input.Ids != null && input.Ids.Count > 0, x => input.Ids.Contains(x.Id))
                    .ToListAsync();
                var result = _citizenTempExcelExporter.ExportToFile(citizens);
                return DataResult.ResultSuccess(result, "Export Success");
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.Message, "Error");
                Logger.Fatal(e.Message, e);
                return data;
            }
        }

        public async Task<object> ImportCitizenTempExcel([FromForm] ImportCitizenInput input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    IFormFile file = input.File;
                    string fileName = file.FileName;
                    string fileExt = Path.GetExtension(fileName);
                    if (fileExt != ".xlsx" && fileExt != ".xls")
                    {
                        return DataResult.ResultError("File not supported", "Error");
                    }

                    string filePath = Path.GetRandomFileName() + fileExt;

                    using (FileStream stream = File.Create(filePath))
                    {
                        await file.CopyToAsync(stream);
                        stream.Close();
                    }

                    var package = new ExcelPackage(new FileInfo(filePath));
                    var worksheet = package.Workbook.Worksheets.First();
                    int rowCount = worksheet.Dimension.End.Row;

                    const int APARTMENT_CODE_INDEX = 1;
                    const int FULLNAME_INDEX = 2;
                    const int SEX_INDEX = 3;
                    const int DATE_OF_BIRTH_INDEX = 4;
                    const int RELATIONSHIP_INDEX = 5;
                    const int IDENTITY_NUMBER_INDEX = 6;
                    const int PHONE_NUMBER_INDEX = 7;
                    const int HOME_ADRESS_INDEX = 8;
                    const int EMAIL_INDEX = 9;
                    const int IS_STAY_INDEX = 10;
                    const int URBAN_CODE_INDEX = 11;
                    const int BUILDING_CODE_INDEX = 12;
                    const int TAX_CODE_INDEX = 13;
                    const int HOMETOWN_INDEX = 14; 

                    var listNew = new List<CitizenTempDto>();
                    var listDupl = new List<CitizenTempDto>();

                    for (var row = 2; row <= rowCount; row++)
                    {
                        string apartmentCode = worksheet.Cells[row, APARTMENT_CODE_INDEX].Text.Trim();
                        string fullName = worksheet.Cells[row, FULLNAME_INDEX].Text.Trim();
                        var sex = (worksheet.Cells[row, SEX_INDEX].Value?.ToString().Trim()) ?? null;
                        var dateOfBirthString = worksheet.Cells[row, DATE_OF_BIRTH_INDEX].Text?.Trim();
                        var dateOfBirth = DateTime.ParseExact(dateOfBirthString, "dd/MM/yyyy",
                                  CultureInfo.InvariantCulture);
                        var identityNumber = (worksheet.Cells[row, IDENTITY_NUMBER_INDEX].Value?.ToString().Trim()) ?? null;
                        var phoneNumber = worksheet.Cells[row, PHONE_NUMBER_INDEX].Value?.ToString();
                        var homeAddress = worksheet.Cells[row, HOME_ADRESS_INDEX].Value?.ToString();
                        var email = worksheet.Cells[row, EMAIL_INDEX].Value?.ToString();
                        string relationShip = worksheet.Cells[row, RELATIONSHIP_INDEX].Text.Trim();
                        var isStay = worksheet.Cells[row, IS_STAY_INDEX].Value?.ToString();
                        string buildingCode = worksheet.Cells[row, BUILDING_CODE_INDEX].Text?.Trim();
                        string urbanCode = worksheet.Cells[row, URBAN_CODE_INDEX].Text.Trim();
                        string taxCode = worksheet.Cells[row, TAX_CODE_INDEX].Text.Trim();
                        var homeTown = worksheet.Cells[row, HOMETOWN_INDEX].Value?.ToString();

                        // Validate apartmentCode và urbanCode
                        if (string.IsNullOrWhiteSpace(apartmentCode) || string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(urbanCode))
                        {
                            throw new Exception($"Error at row {row}: ApartmentCode, FullName, UrbanCode is required.");
                        }
                        bool isStayed;

                        if (isStay != null)
                        {
                            isStayed = isStay.Trim() == "Ở";
                        }
                        else
                        {
                            isStayed = false;
                        }
                        var citizenTemp = new CitizenTempDto()
                        {
                            TenantId = AbpSession.TenantId,
                            CreatorUserId = AbpSession.UserId,
                            ApartmentCode = apartmentCode,
                            FullName = fullName,
                            Gender = sex?.ToString().Trim(),
                            DateOfBirth = dateOfBirth,
                            RelationShip = GetRelationShip(relationShip.ToString().Trim()),
                            IdentityNumber = identityNumber?.ToString().Trim(),
                            PhoneNumber = phoneNumber?.ToString(),
                            Address = homeAddress?.ToString(),
                            Email = email?.ToString(),
                            IsStayed = isStayed,
                            UrbanCode = urbanCode,
                            BuildingCode = buildingCode,
                            TaxCode = taxCode,
                            Hometown = homeTown

                        };


                        var listBuilding = _organizationUnitRepository.GetAllList(x => x.Type == APP_ORGANIZATION_TYPE.BUILDING);
                        var listUrban = _organizationUnitRepository.GetAllList(x => x.Type == APP_ORGANIZATION_TYPE.URBAN);

                        if (!string.IsNullOrEmpty(buildingCode))
                        {
                            var building = listBuilding.FirstOrDefault(x => x.ProjectCode == buildingCode);
                            if (building != null) citizenTemp.BuildingId = building.ParentId;
                        }

                        if (!string.IsNullOrEmpty(urbanCode))
                        {
                            var urban = listUrban.FirstOrDefault(x => x.ProjectCode == urbanCode);
                            if (urban != null)
                            {
                                citizenTemp.UrbanId = urban.ParentId;
                                citizenTemp.UrbanName = urban.DisplayName;
                            }
                        }

                        var existingCitizen = await _citizenTempRepos.FirstOrDefaultAsync(x =>
                            x.ApartmentCode == apartmentCode && x.BuildingId == citizenTemp.BuildingId && x.UrbanId == citizenTemp.UrbanId && x.FullName == citizenTemp.FullName);

                        if (existingCitizen != null)
                        {
                            citizenTemp.Id = existingCitizen.Id;
                            citizenTemp.OwnerGeneration = existingCitizen.OwnerGeneration;
                            listDupl.Add(citizenTemp);
                        }
                        else
                        {
                            listNew.Add(citizenTemp);
                        }
                    }

                    await CreateListCitizenAsync(listNew);

                    // Xóa tệp đã tạo
                    File.Delete(filePath);

                    var res = new
                    {
                        newEntry = listNew,
                        duplicates = listDupl
                    };

                    return DataResult.ResultSuccess(res, "Upload success");
                }
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "Error");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        private static RELATIONSHIP GetRelationShip(string relationship)
        {
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;

            if (currentCulture.Name.Equals("vi"))
            {
                return relationship switch
                {
                    ("Chủ hộ") => RELATIONSHIP.Contractor,
                    ("Vợ") => RELATIONSHIP.Wife,
                    ("Chồng") => RELATIONSHIP.Husband,
                    ("Con gái") => RELATIONSHIP.Daughter,
                    ("Con trai") => RELATIONSHIP.Son,
                    ("Họ hàng") => RELATIONSHIP.Family,
                    ("Cha") => RELATIONSHIP.Father,
                    ("Mẹ") => RELATIONSHIP.Mother,
                    ("Ông") => RELATIONSHIP.Grandfather,
                    ("Bà") => RELATIONSHIP.Grandmother,
                    ("Khách") => RELATIONSHIP.Guest,
                    ("Vợ của khách") => RELATIONSHIP.Wife_Guest,
                    ("Chồng của khách") => RELATIONSHIP.Husband_Guest,
                    ("Con gái của khách") => RELATIONSHIP.Daughter_Guest,
                    ("Con trai của khách") => RELATIONSHIP.Son_Guest,
                    ("Họ hàng của khách") => RELATIONSHIP.Family_Guest,
                    ("Cha của khách") => RELATIONSHIP.Father_Guest,
                    ("Mẹ của khách") => RELATIONSHIP.Mother_Guest,
                    ("Ông của khách") => RELATIONSHIP.Grandfather_Guest,
                    ("Bà của khách") => RELATIONSHIP.Grandmother_Guest,
                    "" => 0,
                    _ => throw new NotImplementedException()
                };
            }
            else if (currentCulture.Name.Equals("en"))
            {
                return relationship switch
                {
                    ("Host") => RELATIONSHIP.Contractor,
                    ("Wife") => RELATIONSHIP.Wife,
                    ("Husband") => RELATIONSHIP.Husband,
                    ("Daughter") => RELATIONSHIP.Daughter,
                    ("Son") => RELATIONSHIP.Son,
                    ("Relative") => RELATIONSHIP.Family,
                    ("Father") => RELATIONSHIP.Father,
                    ("Mother") => RELATIONSHIP.Mother,
                    ("Grandfather") => RELATIONSHIP.Grandfather,
                    ("Grandmother") => RELATIONSHIP.Grandmother,
                    ("Guest") => RELATIONSHIP.Guest,
                    ("Wife guest") => RELATIONSHIP.Wife_Guest,
                    ("Husband guest") => RELATIONSHIP.Husband_Guest,
                    ("Daughter guest") => RELATIONSHIP.Daughter_Guest,
                    ("Son guest") => RELATIONSHIP.Son_Guest,
                    ("Relative guest") => RELATIONSHIP.Family_Guest,
                    ("Father guest") => RELATIONSHIP.Father_Guest,
                    ("Mother guest") => RELATIONSHIP.Mother_Guest,
                    ("Grandfather guest") => RELATIONSHIP.Grandfather_Guest,
                    ("Grandmother guest") => RELATIONSHIP.Grandmother_Guest,
                    "" => 0,
                    _ => throw new NotImplementedException()
                };
            }
            else return 0;
        }


        #endregion

        #region CreateAccount
        public async Task<object> ImportAndCreateAccount([FromForm] ImportCitizenInput input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    IFormFile file = input.File;
                    string fileName = file.FileName;
                    string fileExt = Path.GetExtension(fileName);
                    if (fileExt != ".xlsx" && fileExt != ".xls")
                    {
                        return DataResult.ResultError("File not supported", "Error");
                    }

                    string filePath = Path.GetRandomFileName() + fileExt;

                    using (FileStream stream = File.Create(filePath))
                    {
                        await file.CopyToAsync(stream);
                        stream.Close();
                    }

                    var package = new ExcelPackage(new FileInfo(filePath));
                    var worksheet = package.Workbook.Worksheets.First();
                    int rowCount = worksheet.Dimension.End.Row;

                    const int APARTMENT_CODE_INDEX = 1;
                    const int FULLNAME_INDEX = 2;
                    const int SEX_INDEX = 3;
                    const int DATE_OF_BIRTH_INDEX = 4;
                    const int RELATIONSHIP_INDEX = 5;
                    const int IDENTITY_NUMBER_INDEX = 6;
                    const int PHONE_NUMBER_INDEX = 7;
                    const int HOME_ADRESS_INDEX = 8;
                    const int EMAIL_INDEX = 9;
                    const int IS_STAY_INDEX = 10;
                    const int URBAN_CODE_INDEX = 11;
                    const int BUILDING_CODE_INDEX = 12;
                    const int TAX_CODE_INDEX = 13;
                    var listNew = new List<CitizenTempDto>();
                    var listDupl = new List<CitizenTempDto>();

                    for (var row = 2; row <= rowCount; row++)
                    {
                        string apartmentCode = worksheet.Cells[row, APARTMENT_CODE_INDEX].Text.Trim();
                        string fullName = worksheet.Cells[row, FULLNAME_INDEX].Text.Trim();
                        var sex = (worksheet.Cells[row, SEX_INDEX].Value?.ToString().Trim()) ?? null;
                        var dateOfBirth = (worksheet.Cells[row, DATE_OF_BIRTH_INDEX].Value as DateTime?) ?? null;
                        var identityNumber = (worksheet.Cells[row, IDENTITY_NUMBER_INDEX].Value?.ToString().Trim()) ?? null;
                        var phoneNumber = worksheet.Cells[row, PHONE_NUMBER_INDEX].Value?.ToString();
                        var homeAddress = worksheet.Cells[row, HOME_ADRESS_INDEX].Value?.ToString();
                        var email = worksheet.Cells[row, EMAIL_INDEX].Value?.ToString();
                        string relationShip = worksheet.Cells[row, RELATIONSHIP_INDEX].Text.Trim();
                        var isStay = worksheet.Cells[row, IS_STAY_INDEX].Value?.ToString();
                        string buildingCode = worksheet.Cells[row, BUILDING_CODE_INDEX].Text?.Trim();
                        string urbanCode = worksheet.Cells[row, URBAN_CODE_INDEX].Text.Trim();
                        // Validate apartmentCode và urbanCode
                        if (string.IsNullOrWhiteSpace(apartmentCode)
                            || string.IsNullOrWhiteSpace(fullName)
                            || string.IsNullOrWhiteSpace(urbanCode)
                            || string.IsNullOrWhiteSpace(email))
                        {
                            throw new Exception($"Error at row {row}: ApartmentCode, FullName, UrbanCode, is required.");
                        }

                        if (listNew.Where(x => x.Email == email).Any()) email = "";
                        if (await _userRepos.GetAll().Where(x => x.EmailAddress == email).AnyAsync()) email = "";

                        var citizenTemp = new CitizenTempDto()
                        {
                            TenantId = AbpSession.TenantId,
                            CreatorUserId = AbpSession.UserId,
                            ApartmentCode = apartmentCode,
                            FullName = fullName,
                            Gender = sex?.ToString().Trim(),
                            DateOfBirth = dateOfBirth,
                            RelationShip = GetRelationShip(relationShip.ToString().Trim()),
                            IdentityNumber = identityNumber?.ToString().Trim(),
                            PhoneNumber = phoneNumber?.ToString(),
                            Address = homeAddress?.ToString(),
                            Email = email?.ToString(),
                            IsStayed = true,
                            UrbanCode = urbanCode,
                            BuildingCode = buildingCode,
                        };

                        var listBuilding = _organizationUnitRepository.GetAllList(x => x.Type == APP_ORGANIZATION_TYPE.BUILDING);
                        var listUrban = _organizationUnitRepository.GetAllList(x => x.Type == APP_ORGANIZATION_TYPE.URBAN);

                        if (!string.IsNullOrEmpty(buildingCode))
                        {
                            var building = listBuilding.FirstOrDefault(x => x.ProjectCode == buildingCode);
                            if (building != null) citizenTemp.BuildingId = building.ParentId;
                        }

                        if (!string.IsNullOrEmpty(urbanCode))
                        {
                            var urban = listUrban.FirstOrDefault(x => x.ProjectCode == urbanCode);
                            if (urban != null) citizenTemp.UrbanId = urban.ParentId;
                        }

                        var existingCitizen = await _citizenTempRepos.FirstOrDefaultAsync(x =>
                            x.ApartmentCode == apartmentCode && x.BuildingId == citizenTemp.BuildingId && x.UrbanId == citizenTemp.UrbanId && x.FullName == citizenTemp.FullName && x.Email == citizenTemp.Email);

                        if (existingCitizen != null)
                        {
                            citizenTemp.Id = existingCitizen.Id;
                            citizenTemp.OwnerGeneration = existingCitizen.OwnerGeneration;
                            listDupl.Add(citizenTemp);
                        }
                        else
                        {
                            listNew.Add(citizenTemp);
                        }
                    }

                    await CreateListCitizenAsync(listNew);

                    foreach (var newCitizen in listNew)
                    {
                        var accountId = await CreateUsersFromTempList(newCitizen);
                        await CreateListCitizenVerifiedAsync(newCitizen, accountId);

                        var ctz = await _citizenTempRepos.GetAsync(newCitizen.Id);
                        ctz.AccountId = accountId;
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }



                    // Xóa tệp đã tạo
                    File.Delete(filePath);

                    var res = new
                    {
                        newEntry = listNew,
                        duplicates = listDupl
                    };

                    return DataResult.ResultSuccess(res, "Upload success");
                }
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "Error");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }


        private async Task CreateListCitizenVerifiedAsync(CitizenTempDto input, long accountId)
        {
            try
            {
                var citizen = new Citizen()
                {
                    AccountId = accountId,
                    Address = input.Address,
                    ApartmentCode = input.ApartmentCode,
                    BirthYear = input.DateOfBirth.HasValue ? input.DateOfBirth.Value.Year : 1970,
                    DateOfBirth = input.DateOfBirth,
                    BuildingId = input.BuildingId,
                    CitizenCode = input.CitizenCode,
                    CitizenTempId = input.Id,
                    IdentityNumber = input.IdentityNumber,
                    Gender = input.Gender,
                    Email = input.Email,
                    FullName = input.FullName,
                    ImageUrl = input.ImageUrl,
                    IsStayed = input.IsStayed,
                    IsVoter = input.IsVoter,
                    MemberNum = input.MemberNum,
                    Nationality = input.Nationality,
                    PhoneNumber = input.PhoneNumber,
                    RelationShip = input.RelationShip,
                    State = STATE_CITIZEN.ACCEPTED,
                    TenantId = AbpSession.TenantId,
                    Type = input.Type,
                    UrbanId = input.UrbanId,
                    UrbanCode = input.UrbanCode,
                    BuildingCode = input.BuildingCode
                };
                if (citizen.CitizenCode == null) citizen.CitizenCode = GetUniqueCitizenCode();

                long id = await _citizenRepos.InsertAndGetIdAsync(citizen);
                await _homeMemberManager.UpdateCitizenInHomeMember(citizen, true, AbpSession.TenantId);
                citizen.Id = id;
                CurrentUnitOfWork.SaveChanges();
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "Error");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        private async Task<long> CreateUsersFromTempList(CitizenTempDto input)
        {
            try
            {
                var nameArr = input.FullName.Split(" ");
                var surname = nameArr[0];
                var first_name = input.FullName.Remove(0, surname.Length).Trim();
                var username = nameArr.Length > 1 ? RemoveUnicode(nameArr[nameArr.Length - 1].Replace(" ", "")) + "" + input.ApartmentCode : RemoveUnicode(surname) + "" + input.ApartmentCode;

                var userTemp = new CreateUserDto()
                {
                    DateOfBirth = input.DateOfBirth,
                    Gender = input.Gender,
                    Password = "1234@" + input.ApartmentCode,
                    UserName = username,
                    Name = first_name,
                    Surname = surname,
                    Nationality = input.Nationality,
                    EmailAddress = input.Email
                };

                if (input.Email == null || input.Email == "")
                {
                    userTemp.EmailAddress = userTemp.UserName.ToLower() + "@yootek.com";
                }
                userTemp.RoleNames = new string[1] { "DEFAULTUSER" };

                string originalUsername = userTemp.UserName;

                var user = ObjectMapper.Map<User>(userTemp);

                user.TenantId = AbpSession.TenantId;
                user.IsEmailConfirmed = true;
                user.IsActive = true;

                await _userManager.InitializeOptionsAsync(AbpSession.TenantId);

                CheckErrors(await _userManager.CreateAsync(user, userTemp.Password));

                if (userTemp.RoleNames != null)
                {
                    CheckErrors(await _userManager.SetRolesAsync(user, userTemp.RoleNames));
                }

                await _notificationSubscriptionManager.SubscribeToAllAvailableNotificationsAsync(user.ToUserIdentifier());

                CurrentUnitOfWork.SaveChanges();

                var id = (await _userRepos.FirstOrDefaultAsync(x => x.UserName == user.UserName)).Id;
                return id;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.Message, "Error");
                Logger.Fatal(e.Message, e);
                throw;
            }
        }

        public static string RemoveUnicode(string text)
        {
            string[] arr1 = new string[] { "á", "à", "ả", "ã", "ạ", "â", "ấ", "ầ", "ẩ", "ẫ", "ậ", "ă", "ắ", "ằ", "ẳ", "ẵ", "ặ",
                "đ",
                "é","è","ẻ","ẽ","ẹ","ê","ế","ề","ể","ễ","ệ",
                "í","ì","ỉ","ĩ","ị",
                "ó","ò","ỏ","õ","ọ","ô","ố","ồ","ổ","ỗ","ộ","ơ","ớ","ờ","ở","ỡ","ợ",
                "ú","ù","ủ","ũ","ụ","ư","ứ","ừ","ử","ữ","ự",
                "ý","ỳ","ỷ","ỹ","ỵ",};
            string[] arr2 = new string[] { "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a",
                "d",
                "e","e","e","e","e","e","e","e","e","e","e",
                "i","i","i","i","i",
                "o","o","o","o","o","o","o","o","o","o","o","o","o","o","o","o","o",
                "u","u","u","u","u","u","u","u","u","u","u",
                "y","y","y","y","y",};
            for (int i = 0; i < arr1.Length; i++)
            {
                text = text.Replace(arr1[i], arr2[i]);
                text = text.Replace(arr1[i].ToUpper(), arr2[i].ToUpper());
            }
            return text.ToLower();
        }

        #endregion
    }
}
