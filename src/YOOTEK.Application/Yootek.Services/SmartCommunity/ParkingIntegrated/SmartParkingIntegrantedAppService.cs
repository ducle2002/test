using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.UI;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Services 
{ 

    public interface ISmartParkingIntegrantedAppService : IApplicationService
    {

    }

    public class SmartParkingIntegrantedAppService: YootekAppServiceBase, ISmartParkingIntegrantedAppService
    {

        private readonly IRepository<CitizenTemp, long> _citizenTempRepos;
        private readonly IRepository<IntegratedParkingCustomer, long> _integratedCustomerRepos;

        public SmartParkingIntegrantedAppService(
            IRepository<CitizenTemp, long> citizenTempRepos,
            IRepository<IntegratedParkingCustomer, long> integratedCustomerRepos
            ) 
        {
            _citizenTempRepos = citizenTempRepos;
            _integratedCustomerRepos = integratedCustomerRepos;
        }

        public async Task<object> GetCitizenByApartment(string apartmentCode)
        {
            try
            {
                var query = (from dt in _citizenTempRepos.GetAll()
                             join it in _integratedCustomerRepos.GetAll() on dt.Id equals it.CitizenTempId into tb_it
                             from it in tb_it.DefaultIfEmpty()
                             select new
                             {
                                 Id = dt.Id,
                                 ApartmentCode = dt.ApartmentCode,
                                 IsStayed = dt.IsStayed,
                                 FullName = dt.FullName,
                                 CustomerID = it.CustomerID

                             })
                             .Where(x => x.ApartmentCode == apartmentCode)
                             .AsQueryable();

                var result = await query.ToListAsync();
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


        public async Task<object> CreateCustomerFromCitizenTemp(long? buildingId, long? urbanId )
        {
            try
            {
                

                
                var data = DataResult.ResultSuccess( "Get success!");
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
