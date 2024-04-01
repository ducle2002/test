using Abp.Application.Services;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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
        private readonly IRepository<DigitalServices, long> _digitalServiceRepository;
        private readonly IRepository<DigitalServiceDetails, long> _digitalServiceDetailRepository;
        private readonly IRepository<Citizen, long> _citizenRepository;
        private readonly DigitalServicePaymentUtil _digitalServicePaymentUtil;

        public AdminDigitalServicePaymentAppService(
             IRepository<DigitalServicePayment, long> digitalServicePaymentRepository,
             IRepository<DigitalServiceOrder, long> digitalServiceOrderRepository,
             IRepository<DigitalServices, long> digitalServiceRepository,
             IRepository<DigitalServiceDetails, long> digitalServiceDetailRepository,
             IRepository<Citizen, long> citizenRepository,
             DigitalServicePaymentUtil digitalServicePaymentUtil
            )
        {
            _digitalServicePaymentRepository = digitalServicePaymentRepository;
            _digitalServiceOrderRepository = digitalServiceOrderRepository;
            _digitalServiceRepository = digitalServiceRepository;
            _digitalServiceDetailRepository = digitalServiceDetailRepository;
            _citizenRepository = citizenRepository;
            _digitalServicePaymentUtil = digitalServicePaymentUtil;
        }

        private IQueryable<DigitalServicePaymentDto> QueryDigitalServicePayment()
        {
           return (from pm in _digitalServicePaymentRepository.GetAll()
             join sv in _digitalServiceRepository.GetAll() on pm.ServiceId equals sv.Id into tb_sv
             from sv in tb_sv.DefaultIfEmpty()
             join od in _digitalServiceOrderRepository.GetAll() on pm.OrderId equals od.Id into tb_od
             from od in tb_od.DefaultIfEmpty()
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
                 ApartmentCode = pm.ApartmentCode,
                 Address = od.Address,
                 ServicesText = sv.Title,
                 CustomerName = _citizenRepository.GetAll().Where(x => x.AccountId == od.CreatorUserId).Select(x => x.FullName).FirstOrDefault(),
                 ServiceDetails = od.ServiceDetails
             }).AsQueryable();
        }

        public async Task<DataResult> GetAllAsync(GetAllDigitalServicePaymentInput input)
        {
            try
            {
                IQueryable<DigitalServicePaymentDto> query = QueryDigitalServicePayment()
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
                var data = await QueryDigitalServicePayment().FirstOrDefaultAsync(x => x.Id == id);
                if (data == null) throw new UserFriendlyException("Data not found !");

                data.ArrServiceDetails = !string.IsNullOrEmpty(data.ServiceDetails) ? JsonConvert.DeserializeObject<List<DigitalServiceDetailsGridDto>>(data.ServiceDetails) : new List<DigitalServiceDetailsGridDto>();

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
                var data = await _digitalServicePaymentUtil.HandlePaymentSuccess(dto.OrderId, dto.Amount, dto.Method, dto.Note, null, DigitalServicePaymentStatus.SUCCESS);
              

                return DataResult.ResultSuccess(data, "Insert success !");
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
