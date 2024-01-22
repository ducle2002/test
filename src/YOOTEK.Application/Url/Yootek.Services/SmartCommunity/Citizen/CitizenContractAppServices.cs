using Abp.Application.Services;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Yootek.Yootek.EntityDb.SmartCommunity.QuanLyDanCu.Citizen;
using Yootek.Organizations;
using Yootek.Services.Dto;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Yootek.Services
{
    public interface ICitizenContractAppService : IApplicationService { }
    public class CitizenContractAppService : YootekAppServiceBase, ICitizenContractAppService
    {
        private readonly IRepository<CitizenContract, long> _contractRepo;
        private readonly IRepository<CitizenTemp, long> _tempRepo;
        private readonly IRepository<AppOrganizationUnit, long> _organizationUnitRepo;

        public CitizenContractAppService(
            IRepository<CitizenContract, long> contractRepo,
            IRepository<CitizenTemp, long> tempRepo,
            IRepository<AppOrganizationUnit, long> organizationUnitRepo
        )
        {
            _contractRepo = contractRepo;
            _tempRepo = tempRepo;
            _organizationUnitRepo = organizationUnitRepo;
        }

        public async Task<object> GetAllCitizenContractAsync(CitizenContractQueryDto input)
        {
            try
            {
                var query = (from c in _contractRepo.GetAll()
                             join t in _tempRepo.GetAll() on c.CitizenTempId equals t.Id
                             select new CitizenContractOutputDto()
                             {
                                 ApartmentCode = c.ApartmentCode,
                                 BuildingId = c.BuildingId,
                                 CitizenTempId = c.CitizenTempId,
                                 CreationTime = c.CreationTime,
                                 CreatorUserId = c.CreatorUserId,
                                 DeleterUserId = c.DeleterUserId,
                                 ExpiryDate = c.ExpiryDate,
                                 Id = c.Id,
                                 OrganizationUnitId = c.OrganizationUnitId,
                                 PaymentDay = c.PaymentDay,
                                 ProviderDetails = c.ProviderDetails,
                                 ProviderFullName = c.ProviderFullName,
                                 RenterDetails = c.RenterDetails,
                                 RenterFullName = c.RenterFullName,
                                 RentMoney = c.RentMoney,
                                 StartRentDate = c.StartRentDate,
                                 UrbanId = c.UrbanId,
                                 DateOfBirth = t.DateOfBirth,
                                 Gender = t.Gender,
                             })
                    .WhereIf(input.BuildingId.HasValue, x => x.BuildingId.Value == input.BuildingId.Value)
                    .WhereIf(input.UrbanId.HasValue, x => x.UrbanId.Value == input.UrbanId.Value).AsQueryable();
                var result = query.PageBy(input).ToList();
                if (result != null && result.Count() > 0)
                {
                    foreach (var item in result)
                    {
                        var urbanDf = _organizationUnitRepo.FirstOrDefault(x => x.Type == APP_ORGANIZATION_TYPE.URBAN);
                        if (item.UrbanId.HasValue)
                        {
                            var urban = _organizationUnitRepo.FirstOrDefault(x => x.Id == item.UrbanId);
                            item.UrbanName = urban != null ? urban.DisplayName : urbanDf.DisplayName;
                        }
                        else item.UrbanName = urbanDf.DisplayName;
                    }
                }
                var data = DataResult.ResultSuccess(result, "Get success !", query.Count());
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        [Obsolete]
        public async Task<object> CreateOrUpdateCitizenContractAsync(CitizenContractInputDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                input.TenantId = AbpSession.TenantId;
                if (input.Id > 0)
                {
                    var updateData = await _contractRepo.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);
                        if (updateData.CitizenTempId.HasValue)
                        {
                            var updateTemp = await _tempRepo.GetAsync(updateData.CitizenTempId.Value);
                            if (updateTemp != null)
                            {
                                updateTemp.ApartmentCode = updateData.ApartmentCode;
                                updateTemp.FullName = updateData.RenterFullName;
                                updateTemp.BuildingId = updateData.BuildingId;
                                updateTemp.UrbanId = updateData.UrbanId;
                                updateTemp.OrganizationUnitId = updateData.OrganizationUnitId;
                                updateTemp.TenantId = AbpSession.TenantId;
                                updateTemp.DateOfBirth = input.DateOfBirth;
                                updateTemp.IdentityNumber = input.RenterIdentityNumber;
                                updateTemp.PhoneNumber = input.RenterPhoneNumber;
                                updateTemp.Gender = input.Gender;
                                await _tempRepo.UpdateAsync(updateTemp);
                            }
                            else
                            {
                                var newCitizenTemp = new CitizenTempDto()
                                {
                                    ApartmentCode = updateData.ApartmentCode,
                                    FullName = updateData.RenterFullName,
                                    BuildingId = updateData.BuildingId,
                                    UrbanId = updateData.UrbanId,
                                    OrganizationUnitId = updateData.OrganizationUnitId,
                                    TenantId = updateData.TenantId,
                                    DateOfBirth = input.DateOfBirth,
                                    IdentityNumber = input.RenterIdentityNumber,
                                    PhoneNumber = input.RenterPhoneNumber,
                                    Gender = input.Gender,
                                };
                                updateData.CitizenTempId = await _tempRepo.InsertAndGetIdAsync(newCitizenTemp);
                            }
                        }
                        else
                        {
                            var newCitizenTemp = new CitizenTempDto()
                            {
                                ApartmentCode = updateData.ApartmentCode,
                                FullName = updateData.RenterFullName,
                                BuildingId = updateData.BuildingId,
                                UrbanId = updateData.UrbanId,
                                OrganizationUnitId = updateData.OrganizationUnitId,
                                TenantId = updateData.TenantId,
                                DateOfBirth = input.DateOfBirth,
                                IdentityNumber = input.RenterIdentityNumber,
                                PhoneNumber = input.RenterPhoneNumber,
                                Gender = input.Gender,
                            };
                            updateData.CitizenTempId = await _tempRepo.InsertAndGetIdAsync(newCitizenTemp);
                        }

                        await _contractRepo.UpdateAsync(updateData);
                    }
                    mb.statisticMetris(t1, 0, "Ud_citizen");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {
                    var insertData = input.MapTo<CitizenContract>();
                    var newCitizenTemp = new CitizenTempDto()
                    {
                        ApartmentCode = insertData.ApartmentCode,
                        FullName = insertData.RenterFullName,
                        BuildingId = insertData.BuildingId,
                        UrbanId = insertData.UrbanId,
                        OrganizationUnitId = insertData.OrganizationUnitId,
                        TenantId = insertData.TenantId,
                        DateOfBirth = input.DateOfBirth,
                        IdentityNumber = input.RenterIdentityNumber,
                        PhoneNumber = input.RenterPhoneNumber,
                        Gender = input.Gender,
                    };
                    insertData.CitizenTempId = await _tempRepo.InsertAndGetIdAsync(newCitizenTemp);
                    await _contractRepo.InsertAsync(insertData);
                    mb.statisticMetris(t1, 0, "is_citizen");
                    var data = DataResult.ResultSuccess(insertData, "Insert success !");
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

        public async Task<object> DeleteCitizenContract(long id)
        {
            try
            {

                await _contractRepo.DeleteAsync(id);
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

        public object DeleteMultipleContracts([FromBody] List<long> ids)
        {
            try
            {
                if (ids.Count == 0) return DataResult.ResultError("Error", "Empty input!");
                var tasks = new List<Task>();
                foreach (var id in ids)
                {
                    var task = DeleteCitizenContract(id);
                    tasks.Add(task);
                }
                Task.WaitAll(tasks.ToArray());
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
    }
}
