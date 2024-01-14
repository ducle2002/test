using Abp.Application.Services;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.UI;
using Yootek.Application;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Yootek.Yootek.Services.Yootek.SmartCommunity.ApartmentDiscount;
using Yootek.Services.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Yootek.Services
{
    public interface IApartmentDiscountAppService : IApplicationService
    {
        Task<object> GetAllAsync(GetAllInput input);
        Task<object> GetByIdAsync(long id);
        Task<object> Create(ApartmentDiscountDto input);
        Task<object> Update(ApartmentDiscountDto input);
        Task<object> Delete(long id);
        Task<object> DeleteMany([FromBody] List<long> ids);
    }
    public class ApartmentDiscountAppService : YootekAppServiceBase, IApartmentDiscountAppService
    {
        private readonly IRepository<ApartmentDiscount, long> _apartmentDiscountRepository;
        public ApartmentDiscountAppService(
            IRepository<ApartmentDiscount, long> apartmentDiscountRepository
            )
        {
            _apartmentDiscountRepository = apartmentDiscountRepository;
        }
        public async Task<object> GetAllAsync(GetAllInput input)
        {
            try
            {
                var query = _apartmentDiscountRepository.GetAll()
                                 .ApplySearchFilter(input.Keyword, x => x.Name, x => x.ApartmentCode)
                                 .AsQueryable();

                var result = await query.OrderBy(x => x.ApartmentCode).ThenByDescending(x => x.CreationTime)
                    .Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
                return DataResult.ResultSuccess(result, "Get success!", query.Count());
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> GetAllByApartmentCodeAsync(GetAllByApartmentCodeInput input)
        {
            try
            {
                var result = await _apartmentDiscountRepository.GetAllListAsync(x => x.ApartmentCode == input.ApartmentCode);
                return DataResult.ResultSuccess(result, "Get success !");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<object> GetByIdAsync(long id)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var apartment = await _apartmentDiscountRepository.FirstOrDefaultAsync(id);
                    return DataResult.ResultSuccess(apartment.MapTo<ApartmentDto>(), "Get apartment detail success!");
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<object> Create(ApartmentDiscountDto input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var apartmentOrg = await _apartmentDiscountRepository.FirstOrDefaultAsync(x => x.ApartmentCode == input.ApartmentCode);
                    if (apartmentOrg != null) throw new UserFriendlyException(409, "Apartment is exist");
                    var apartment = input.MapTo<ApartmentDiscount>();
                    apartment.TenantId = AbpSession.TenantId;

                    await _apartmentDiscountRepository.InsertAsync(apartment);
                    return DataResult.ResultSuccess(true, "Insert success!");
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> CreateOrUpdateDiscount(CreateOrUpdateApartmentDiscountInput input)
        {
            try
            {
                if (input.ApartmentCode != null && input.Discounts != null)
                {
                    foreach (var discount in input.Discounts)
                    {
                        discount.ApartmentCode = input.ApartmentCode;
                        discount.BuildingId = input.BuildingId;
                        discount.UrbanId = input.UrbanId;

                        if (discount.IsChecked && discount.Id > 0)
                        {
                            var apartment = discount.MapTo<ApartmentDiscount>();
                            apartment.TenantId = AbpSession.TenantId;

                            await _apartmentDiscountRepository.UpdateAsync(apartment);
                        }
                        else if (discount.IsChecked && discount.Id == 0)
                        {
                            var apartment = discount.MapTo<ApartmentDiscount>();
                            apartment.TenantId = AbpSession.TenantId;

                            await _apartmentDiscountRepository.InsertAsync(apartment);
                        }
                        else
                        if (!discount.IsChecked && discount.Id > 0)
                        {
                            await _apartmentDiscountRepository.DeleteAsync(discount.Id);
                        }


                    }
                }

                return DataResult.ResultSuccess(true, "Update success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> Update(ApartmentDiscountDto input)
        {
            try
            {
                var updateData = await _apartmentDiscountRepository.FirstOrDefaultAsync(input.Id)
                          ?? throw new Exception("Apartment not found!");
                input.MapTo(updateData);
                await _apartmentDiscountRepository.UpdateAsync(updateData);
                return DataResult.ResultSuccess(true, "Update success !");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> Delete(long id)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    await _apartmentDiscountRepository.DeleteAsync(id);
                    return DataResult.ResultSuccess("Delete success!");
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> DeleteMany([FromBody] List<long> ids)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    await _apartmentDiscountRepository.DeleteAsync(x => ids.Contains(x.Id));
                    return DataResult.ResultSuccess("Delete list apartment success!");
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
    }
}
