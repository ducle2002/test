using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yootek;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Yootek.Services;
using YOOTEK.EntityDb.IMAX.DichVu.DigitalServices;

namespace YOOTEK.Yootek.Services
{
    public interface IAdminDigitalServicePaymentAppService : IApplicationService
    {

    }

    public class AdminDigitalServicePaymentAppService : YootekAppServiceBase, IAdminDigitalServicePaymentAppService
    {
        private readonly IRepository<DigitalServicePayment, long> _digitalServicePaymentRepository;
        private readonly IRepository<DigitalServiceOrder, long> _digitalServiceOrderRepository;

        public AdminDigitalServicePaymentAppService(
             IRepository<DigitalServicePayment, long> digitalServicePaymentRepository,
             IRepository<DigitalServiceOrder, long> digitalServiceOrderRepository
            )
        {
            _digitalServicePaymentRepository = digitalServicePaymentRepository;
            _digitalServiceOrderRepository = digitalServiceOrderRepository;
        }

        public async Task<DataResult> GetAllAsync(GetAllDigitalServicePaymentInput input)
        {
            try
            {
                IQueryable<DigitalServicePaymentDto> query = (from pm in _digitalServicePaymentRepository.GetAll()
                                                        select new DigitalServicePaymentDto
                                                        {
                                                            Id = pm.Id,
                                                            Amount = pm.Amount, 
                                                            Code = pm.Code,
                                                            UrbanId = pm.UrbanId,
                                                            BuildingId = pm.BuildingId,
                                                            Method = pm.Method,
                                                            Note = pm.Note,
                                                            OrderId = pm.OrderId,
                                                            Properties = pm.Properties,
                                                            ServiceId = pm.ServiceId,
                                                            Status = pm.Status,
                                                            CreationTime = pm.CreationTime,
                                                            TenantId = pm.TenantId,
                                                            ApartmentCode = pm.ApartmentCode

                                                        })
                                                        .WhereIf(!string.IsNullOrEmpty(input.Keyword),
                                                        x => x.ApartmentCode.ToLower().Contains(input.Keyword.ToLower())
                                                        || x.Code.ToLower().Contains(input.Keyword.ToLower())
                                                        || x.Note.ToLower().Contains(input.Keyword.ToLower()))
                                                        .WhereIf(input.UrbanId > 0, x => x.UrbanId == input.UrbanId)
                                                        .WhereIf(input.BuildingId > 0, x => x.BuildingId == input.BuildingId);

                var data = await query.PageBy(input).ToListAsync();
                return DataResult.ResultSuccess(data,"", query.Count());
            }
            catch(Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<DataResult> GetById(long id)
        {
            try
            {
                var data = await _digitalServicePaymentRepository.FirstOrDefaultAsync(id);
                if (data == null) throw new UserFriendlyException("Data not found !");

                return DataResult.ResultSuccess(ObjectMapper.Map<DigitalServicePaymentDto>(data), "");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<DataResult> CreateAsync(CreateDigitalServicePaymentDto dto)
        {
            try
            {
                var insertData = ObjectMapper.Map<DigitalServicePayment>(dto);
                await _digitalServicePaymentRepository.InsertAndGetIdAsync(insertData);

                return DataResult.ResultSuccess(insertData, "Insert success !");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<DataResult> UpdateAsync(UpdateDigitalServicePaymentDto dto)
        {
            try
            {
                var data = await _digitalServicePaymentRepository.FirstOrDefaultAsync(dto.Id);
                if (data == null) throw new UserFriendlyException("Data not found !");
                data = ObjectMapper.Map(dto, data);
                await _digitalServicePaymentRepository.UpdateAsync(data);

                return DataResult.ResultSuccess(data, "Update success !");
            }
            catch(Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        //public async Task<DataResult> UpdateStateAsync()
        //{

        //}

        public async Task<DataResult> DeleteAsync(long id)
        {
            try
            {
                await _digitalServicePaymentRepository.DeleteAsync(id);

                return DataResult.ResultSuccess ("Delete success !");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<DataResult> DeleteMultipleAsync(List<long> ids) 
        {
            try
            {
                await _digitalServicePaymentRepository.DeleteAsync(x => ids.Contains(x.Id));

                return DataResult.ResultSuccess("Delete success !");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }
    }
}
