using Abp.Application.Services;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using AutoMapper;
using DocumentFormat.OpenXml.Drawing.Charts;
using Yootek.Authorization;
using Yootek.Authorization.Users;
using Yootek.Common.DataResult;
using Yootek.Common.Enum;
using Yootek.EntityDb;
using Yootek.Notifications;
using Yootek.Organizations;
using Yootek.QueriesExtension;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Services
{
    public interface IAdminBillPrepaymentAppService : IApplicationService
    {
        Task<DataResult> GetAllAsync(GetBillPrepaymentInput input);
        Task<DataResult> CreateAsync(BillPrepaymentDto input);
        Task<DataResult> UpdateAsync(BillPrepaymentDto input);
        Task<DataResult> DeleteAsync(long id);
    }

    [AbpAuthorize]
    public class AdminBillPrepaymentAppService: YootekAppServiceBase, IAdminBillPrepaymentAppService
    {
        private readonly IRepository<UserBill, long> _userBillRepo;
        private readonly IRepository<CitizenTemp, long> _citizenTempRepo;
        private readonly IRepository<UserBillPayment, long> _userBillPaymentRepo;
        private readonly IRepository<BillPrepayment, long> _billPrepaymentRepository;
        private readonly IRepository<AppOrganizationUnit, long> _organizationUnitRepository;
        public AdminBillPrepaymentAppService(
            IRepository<UserBill, long> userBillRepo,
            IRepository<UserBillPayment, long> userBillPaymentRepo,
            IRepository<CitizenTemp, long> citizenTempRepo,
            IRepository<AppOrganizationUnit, long> organizationUnitRepository,
            IRepository<BillPrepayment, long> billPrepaymentRepository
            ) {
            _userBillRepo = userBillRepo;
            _userBillPaymentRepo = userBillPaymentRepo;
            _organizationUnitRepository = organizationUnitRepository;
            _citizenTempRepo = citizenTempRepo;
            _billPrepaymentRepository = billPrepaymentRepository;
        }

        public async Task<DataResult> GetAllAsync(GetBillPrepaymentInput input)
        {
            try
            {
                List<long> buIds = UserManager.GetAccessibleBuildingOrUrbanIds();
                var query = _billPrepaymentRepository.GetAll()
                    .WhereByBuildingOrUrbanIf(!IsGranted(IOCPermissionNames.Data_Admin), buIds);
                var data = await query.PageBy(input).ToListAsync();

                var result = data.MapTo<List<BillPrepaymentDto>>();
                if(result != null)
                {
                    foreach(var item in result)
                    {
                        if(item.BillType == BillType.Manager) { item.BillTypeName = "Phí quản lý"; }
                        if (item.BillType == BillType.Parking) { item.BillTypeName = "Phí xe tháng"; }
                    }
                }
                return DataResult.ResultSuccess(result, "Get success", query.Count());
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "Error");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        [Obsolete]
        public async Task<object> GetById(long id)
        {
            try
            {
                var data = await _billPrepaymentRepository.GetAsync(id);
                var res = data.MapTo<BillPrepaymentDto>();
                if(res != null)
                {
                    if (res.BillType == BillType.Manager) { res.BillTypeName = "Phí quản lý"; }
                    if (res.BillType == BillType.Parking) { res.BillTypeName = "Phí xe tháng"; }
                }
                return DataResult.ResultSuccess(res, "Success!");
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "Error");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<DataResult> CreateAsync(BillPrepaymentDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                input.TenantId = AbpSession.TenantId;

                var insertInput = input.MapTo<BillPrepayment>();
                insertInput.State = BillPrepaymentState.Active;
                var listId = new List<long>();
                if(input.NumberPeriod > 0)
                {
                    var period = DateTime.Now;
                    for (var i = 0; i < input.NumberPeriod; i++)
                    {
                       
                        if(input.StartPeriod.HasValue)
                        {
                            period = input.StartPeriod.Value.AddMonths(i);
                        }
                        else
                        {
                            period = period.AddMonths(1);
                        }
                        var userBill1 = new UserBill();
                        userBill1.Title =  $"Hóa đơn tháng {period.Month}/{period.Year}";
                        userBill1.UrbanId = input.UrbanId ?? null;
                        userBill1.BuildingId = input.BuildingId ?? null;
                        userBill1.ApartmentCode = input.ApartmentCode;
                        userBill1.Period = period;
                        userBill1.DueDate = period;
                        userBill1.TenantId = AbpSession.TenantId;
                        userBill1.CitizenTempId = input.CitizenTempId;
                        userBill1.BillType = input.BillType;
                        userBill1.Status = UserBillStatus.Paid;
                        userBill1.CarNumber = input.CarNumber;
                        userBill1.MotorbikeNumber = input.MotorbikeNumber;
                        userBill1.BicycleNumber = input.BicycleNumber;
                        userBill1.OtherVehicleNumber = input.OtherVehicleNumber;


                        userBill1.TotalIndex = input.TotalIndex;
                        userBill1.LastCost = input.TotalPaid / input.NumberPeriod;
                        if(!string.IsNullOrEmpty(input.Vehicles))
                        {
                            userBill1.Properties = JsonConvert.SerializeObject(new
                            {
                                customerName = input.CustomerName,
                                formulas = input.Formulas,
                                vehicles = JsonConvert.DeserializeObject<object>(input.Vehicles),
                            });
                        }
                        else
                        {
                            userBill1.Properties = JsonConvert.SerializeObject(new
                            {
                                customerName = input.CustomerName,
                                formulas = input.Formulas,
                                vehicles = JsonConvert.DeserializeObject<object>("[]"),
                            });
                        }

                        var id =   await _userBillRepo.InsertAndGetIdAsync(userBill1);
                        listId.Add(id);
                        userBill1.Code = "HD" + userBill1.Id + (period.Month < 10 ? "0" + period.Month : period.Month) + "" + period.Year;
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }
                }

                insertInput.UserBillIds = JsonConvert.SerializeObject(listId);
                await _billPrepaymentRepository.InsertAsync(insertInput);
                mb.statisticMetris(t1, 0, "is_billPrepayment");
                var data = DataResult.ResultSuccess(insertInput, "Insert success !");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "Error");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<DataResult> UpdateAsync(BillPrepaymentDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                input.TenantId = AbpSession.TenantId;

                var updateData = await _billPrepaymentRepository.GetAsync(input.Id);
                if (updateData != null)
                {
                    await DeleteUserBillPrepayment(updateData.UserBillIds);
                    var listId = new List<long>();
                    if (input.NumberPeriod > 0)
                    {
                        var period = DateTime.Now;
                        for (var i = 0; i < input.NumberPeriod; i++)
                        {

                            if (input.StartPeriod.HasValue)
                            {
                                period = input.StartPeriod.Value.AddMonths(i);
                            }
                            else
                            {
                                period = period.AddMonths(i);
                            }
                            var userBill1 = new UserBill();
                            userBill1.Title = $"Hóa đơn tháng {period.Month}/{period.Year}";
                            userBill1.UrbanId = input.UrbanId ?? null;
                            userBill1.BuildingId = input.BuildingId ?? null;
                            userBill1.ApartmentCode = input.ApartmentCode;
                            userBill1.Period = period;
                            userBill1.DueDate = period;
                            userBill1.TenantId = AbpSession.TenantId;
                            userBill1.CitizenTempId = input.CitizenTempId;
                            userBill1.BillType = input.BillType;
                            userBill1.Status = UserBillStatus.Paid;
                            userBill1.CarNumber = input.CarNumber;
                            userBill1.MotorbikeNumber = input.MotorbikeNumber;
                            userBill1.BicycleNumber = input.BicycleNumber;
                            userBill1.OtherVehicleNumber = input.OtherVehicleNumber;
                            userBill1.TotalIndex = input.TotalIndex;
                            userBill1.LastCost = input.TotalPaid / input.NumberPeriod;
                            userBill1.Properties = JsonConvert.SerializeObject(new
                            {
                                customerName = input.CustomerName,
                                formulas = input.Formulas,
                                vehicles = input.Vehicles,
                            });

                            var id = await _userBillRepo.InsertAndGetIdAsync(userBill1);
                            listId.Add(id);
                            userBill1.Code = "HD" + userBill1.Id + (period.Month < 10 ? "0" + period.Month : period.Month) + "" + period.Year;
                            await CurrentUnitOfWork.SaveChangesAsync();
                        }
                    }
                   
                    input.MapTo(updateData);
                    await _billPrepaymentRepository.UpdateAsync(updateData);
                }
                mb.statisticMetris(t1, 0, "Ud_billPrepayment");

                var data = DataResult.ResultSuccess(updateData, "Update success !");
                return data;

            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "Error");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<DataResult> DeleteAsync(long id)
        {
            try
            {
                var item = await _billPrepaymentRepository.GetAsync(id);
                if(item != null)
                {
                    await _billPrepaymentRepository.DeleteAsync(id);
                    await DeleteUserBillPrepayment(item.UserBillIds);
                }
                
                var data = DataResult.ResultSuccess(null, "Delete success");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "Error");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> DeleteMultiple([FromBody] List<long> ids)
        {
            try
            {
                if (ids.Count == 0) return DataResult.ResultError("Error", "Empty input!");
                var tasks = new List<Task>();
                foreach (var id in ids)
                {
                    var task = DeleteAsync(id);
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

        private async Task DeleteUserBillPrepayment(string listIds)
        {
            try
            {
                var billIds = JsonConvert.DeserializeObject<List<long>>(listIds);
                if(billIds != null && billIds.Count > 0)
                {
                    await _userBillRepo.DeleteAsync(x => billIds.Contains(x.Id));
                }
            }
            catch { }
        }


    }
}
