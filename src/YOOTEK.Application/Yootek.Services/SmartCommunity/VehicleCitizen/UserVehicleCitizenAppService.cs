using Abp.Application.Services;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Yootek.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Yootek.Services.Yootek.SmartCommunity.VehicleCitizen
{
    public interface IUserVehicleCitizenAppService: IApplicationService { }
    public class UserVehicleCitizenAppService: YootekAppServiceBase, IUserVehicleCitizenAppService
    {
        private readonly IRepository<CitizenVehicle, long> _citizenVehicleRepos;
        private readonly IRepository<CitizenTemp, long> _citizenTempRepos;
        private readonly IRepository<CitizenParking, long> _citizenParkingRepos;
        public UserVehicleCitizenAppService(
            IRepository<CitizenVehicle, long> citizenVehicleRepos,
            IRepository<CitizenTemp, long> citizenTempRepos,
            IRepository<CitizenParking, long> citizenParkingRepos)
        {
            _citizenVehicleRepos = citizenVehicleRepos;
            _citizenTempRepos = citizenTempRepos;
            _citizenParkingRepos = citizenParkingRepos;
        }

        public async Task<object> GetVehicleByApartmentCode(GetVehicleByApartmentCode input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var query = (from vh in _citizenVehicleRepos.GetAll()
                                 where vh.ApartmentCode.ToUpper().Contains(input.ApartmentCode.ToUpper())
                                 select new CitizenVehicleDto()
                                 {
                                     Id = vh.Id,
                                     ApartmentCode = vh.ApartmentCode,
                                     BuildingId = vh.BuildingId,
                                     CitizenTempId = vh.CitizenTempId,
                                     CreationTime = vh.CreationTime,
                                     CreatorUserId = vh.CreatorUserId,
                                     Description = vh.Description,
                                     UrbanId = vh.UrbanId,
                                     VehicleCode = vh.VehicleCode,
                                     VehicleName = vh.VehicleName,
                                     VehicleType = vh.VehicleType,
                                     TenantId = vh.TenantId,
                                     State = vh.State,
                                     ExpirationDate = vh.ExpirationDate,
                                     RegistrationDate = vh.RegistrationDate,
                                 }).AsQueryable();
                    var paginatedData = await query.PageBy(input).ToListAsync();

                    return DataResult.ResultSuccess(paginatedData, "Get success!", query.Count());
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> GetListParkingAsync(long urbanId)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var query = _citizenParkingRepos.GetAll();
                    var data = await query.Where(x => x.UrbanId == urbanId || x.UrbanId == null)    
                        .ToListAsync();
                    var result = data.ToList();

                    return DataResult.ResultSuccess(result, "Get success!", data.Count());
                }
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception!");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> GetById(long id)
        {
            try
            {
                var data = await _citizenVehicleRepos.GetAsync(id);
                return DataResult.ResultSuccess(data, "Success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }

        }

        [Obsolete]
        public async Task<object> UpdateAsync(CitizenVehicleDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                input.TenantId = AbpSession.TenantId;
                var updateData = await _citizenVehicleRepos.GetAsync(input.Id);
                if (updateData != null)
                {
                    input.MapTo(updateData);

                    await _citizenVehicleRepos.UpdateAsync(updateData);
                }
                mb.statisticMetris(t1, 0, "ud_vehicle");

                var data = DataResult.ResultSuccess(updateData, "Update success!");
                return data;
            }
            catch (Exception e)
            {
                Logger.Info(e.ToString());

                var data = DataResult.ResultError(e.ToString(), "Exception!");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        [Obsolete]
        public async Task<object> CreateAsync(CitizenVehicleDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                input.TenantId = AbpSession.TenantId;

                var insertInput = input.MapTo<CitizenVehicle>();
                insertInput.State = CitizenVehicleState.WAITING;

                long id = await _citizenVehicleRepos.InsertAndGetIdAsync(insertInput);
                insertInput.Id = id;

                mb.statisticMetris(t1, 0, "is_vehicle");

                var data = DataResult.ResultSuccess(insertInput, "Insert success!");
                return data;


            }
            catch (Exception e)
            {
                Logger.Info(e.ToString());

                var data = DataResult.ResultError(e.ToString(), "Exception!");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        [Obsolete]
        public async Task<object> CreateListAsync(RegisterCitizenVehicleInput input)
        {
            try
            {
                var insertList = new List<CitizenVehicle>();
                foreach (var vh in input.ListVehicle)
                {
                    var insertData = vh.MapTo<CitizenVehicle>();
                    insertData.State = CitizenVehicleState.WAITING;
                    insertData.CitizenTempId = input.CitizenTempId;
                    insertData.ApartmentCode = input.ApartmentCode;
                    insertData.BuildingId = input.BuildingId;
                    insertData.UrbanId = input.UrbanId;
                    long id = await _citizenVehicleRepos.InsertAndGetIdAsync(insertData);
                    insertData.Id = id;

                }
                var data = DataResult.ResultSuccess(insertList, "Success!", insertList.Count());
                return data;
            } catch(Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
    }
}
