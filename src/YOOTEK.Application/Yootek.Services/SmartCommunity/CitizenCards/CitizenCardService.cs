using Abp;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Abp.UI;
using Yootek.Common.DataResult;
using Yootek.Core.Dto;
using Yootek.EntityDb;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Services
{
    public interface ICitizenCardService : IApplicationService
    {
        Task<object> GetAllCitizenCardsAsync(CitizenCardInput input);
        Task<object> CreateOrUpdateCitizenCardAsync(CitizenCardDto input);
        Task<object> DeleteCitizenCardAsync(long id);
        Task<object> ChangeCitizenCardLockStatusAsync(long id);
    }

    [AbpAuthorize]
    public class CitizenCardService : YootekAppServiceBase, ICitizenCardService
    {
        private readonly IRepository<CitizenCard, long> _citizenCardRepo;
        private readonly IRepository<Citizen, long> _citizenRepo;
        private readonly ICitizenCardListExcelExporter _citizenCardListExcelExporter;
        public CitizenCardService(IRepository<CitizenCard, long> CitizenCardRepo,
            IRepository<Citizen, long> CitizenRepo,
            ICitizenCardListExcelExporter citizenCardListExcelExporter)
        {
            _citizenCardRepo = CitizenCardRepo;
            _citizenRepo = CitizenRepo;
            _citizenCardListExcelExporter = citizenCardListExcelExporter;
        }

      
        public async Task<object> CreateOrUpdateCitizenCardAsync(CitizenCardDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                input.TenantId = AbpSession.TenantId;
                if (input.Id > 0)
                {
                    var updateCard = await _citizenCardRepo.GetAsync(input.Id);
                    if (updateCard != null)
                    {
                        input.MapTo(updateCard);
                        await _citizenCardRepo.UpdateAsync(updateCard);
                    }
                    else
                    {
                        var resultFailed = DataResult.ResultFail("Cannot Update");
                        return resultFailed;
                    }

                    var resultSuccess = DataResult.ResultSuccess(updateCard, "Update Successfully");
                    return resultSuccess;
                }
                else
                {
                    var inputCard = input.MapTo<CitizenCard>();
                    var data = await _citizenCardRepo.InsertAsync(inputCard);
                    var result = DataResult.ResultSuccess(inputCard, "Inserted successfully");
                    return result;
                }
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.StackTrace, "Failed!");
                Logger.Fatal(e.StackTrace);
                return data;
            }
        }

        public async Task<object> DeleteCitizenCardAsync(long id)
        {
            try
            {
                await _citizenCardRepo.DeleteAsync(id);
                var result = DataResult.ResultSuccess("Deleted Successfully");
                return result;
            }
            catch (Exception e)
            {
                var result = DataResult.ResultError(e.Message, "Delete failed");
                Logger.Fatal(e.StackTrace);
                return result;
            }
        }

        public async Task<object> GetAllCitizenCardsAsync(CitizenCardInput input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var query = (from card in _citizenCardRepo.GetAll()
                                 join cit in _citizenRepo.GetAll() on card.CitizenCode equals cit.Id
                                 select new CitizenCardOutput()
                                 {
                                     TenantId = card.TenantId,
                                     ApartmentCode = cit.ApartmentCode,
                                     BuildingCode = cit.BuildingCode,
                                     CitizenCode = card.CitizenCode,
                                     CreationTime = card.CreationTime,
                                     CreatorUserId = card.CreatorUserId,
                                     DeleterUserId = card.DeleterUserId,
                                     DeletionTime = card.DeletionTime,
                                     Email = cit.Email,
                                     Id = card.Id,
                                     IsDeleted = card.IsDeleted,
                                     IsLocked = card.IsLocked,
                                     LastModificationTime = card.LastModificationTime,
                                     LastModifierUserId = card.LastModifierUserId,
                                     OrganizationUnitId = card.OrganizationUnitId,
                                     Phone = cit.PhoneNumber,
                                     FullName = cit.FullName,
                                     Address = cit.Address,
                                     DateOfBirth = cit.DateOfBirth,
                                     Gender = cit.Gender,
                                     ImageUrl = cit.ImageUrl,
                                     BirthYear = cit.BirthYear,
                                     OtherPhones = cit.OtherPhones,
                                     Nationality = cit.Nationality,
                                     IdentityNumber = cit.IdentityNumber
                                 })
                                .Where(x => x.OrganizationUnitId == input.OrganizationUnitId)
                                .AsQueryable();
                    //var a = query.ToList();
                    /*if (input.Type > 0)
                    {
                        query = query.Where(x => x.Type == input.Type);
                    }*/
                    switch (input.IsLocked)
                    {
                        case true:
                            query = query.Where(x => x.IsLocked == true);
                            break;
                        case false:
                            query = query.Where(x => x.IsLocked == false);
                            break;
                    }
                    if (input.Keyword != null)
                    {
                        query = query.Where(x => (x.FullName.Contains(input.Keyword)));

                    }

                    var count = await query.CountAsync();
                    var data = await query.Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
                    var result = DataResult.ResultSuccess(data, "Success", count);
                    return result;
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e.StackTrace);
                var result = DataResult.ResultError(e.Message, "Failed");
                return result;
            }
        }

        public async Task<object> ChangeCitizenCardLockStatusAsync(long id)
        {
            try
            {
                string message = "";
                var lockCard = await _citizenCardRepo.GetAsync(id);
                lockCard.IsLocked = !lockCard.IsLocked;
                await _citizenCardRepo.UpdateAsync(lockCard);
                if (lockCard.IsLocked) message = "Locked";
                else message = "Unlocked";
                var result = DataResult.ResultSuccess(lockCard, message);
                return result;

            }
            catch (Exception e)
            {
                var result = DataResult.ResultError(e.Message, "Lock failed");
                Logger.Fatal(e.StackTrace);
                return result;
            }
        }

      
        public async Task<object> CreateListCardAsync(List<CitizenCardDto> input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                if (input != null)
                {
                    var index = 0;
                    foreach (var obj in input)
                    {
                        index++;
                        obj.TenantId = AbpSession.TenantId;
                        var cardInput = obj.MapTo<CitizenCard>();
                        await _citizenCardRepo.InsertAsync(cardInput);
                    }
                }
                await CurrentUnitOfWork.SaveChangesAsync();
                mb.statisticMetris(t1, 0, "admin_islist_obj");
                var data = DataResult.ResultSuccess("Insert success !");
                return data;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<FileDto> GetCardsToExcel(CitizenCardInput input)
        {
            var cards = await QueryDataFeedback(input).AsNoTracking().OrderByDescending(al => al.CreationTime)
                .ToListAsync();
            return _citizenCardListExcelExporter.ExportToFile(cards);
        }

        protected IQueryable<CitizenCardDto> QueryDataFeedback(CitizenCardInput input)
        {
            var query = (from card in _citizenCardRepo.GetAll()
                         join cit in _citizenRepo.GetAll() on card.CitizenCode equals cit.Id
                         select new CitizenCardDto()
                         {
                             TenantId = card.TenantId,
                             CitizenCode = card.CitizenCode,
                             CreationTime = card.CreationTime,
                             CreatorUserId = card.CreatorUserId,
                             DeleterUserId = card.DeleterUserId,
                             DeletionTime = card.DeletionTime,
                             Id = card.Id,
                             IsDeleted = card.IsDeleted,
                             IsLocked = card.IsLocked,
                             LastModificationTime = card.LastModificationTime,
                             LastModifierUserId = card.LastModifierUserId,
                             OrganizationUnitId = card.OrganizationUnitId,
                         })
                                .Where(x => x.OrganizationUnitId == input.OrganizationUnitId)
                                .AsQueryable();
            return query;
        }
    }
}
