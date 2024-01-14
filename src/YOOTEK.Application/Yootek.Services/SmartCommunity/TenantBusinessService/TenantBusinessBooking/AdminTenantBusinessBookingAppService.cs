using Abp;
using Abp.Application.Services;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.RealTime;
using Abp.UI;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Yootek.Notifications;
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
    public interface IAdminTenantBusinessBookingAppService : IApplicationService
    {
        Task<object> GetAllBookingAsync(GetBookingInput input);

    }

    public class AdminTenantBusinessBookingAppService : YootekAppServiceBase, IAdminTenantBusinessBookingAppService
    {
        private readonly IRepository<Booking, long> _bookingRepos;
        private readonly IRepository<ItemBooking, long> _itemBookingRepos;
        private readonly IRepository<ObjectPartner, long> _storeBookingRepos;
        private readonly IRepository<LocalService, long> _localServiceRepos;
        private readonly IOnlineClientManager _onlineClientManager;
      //  private readonly ITenantBookingCommunicator _tenantBookingCommunicator;
        private readonly IRepository<BookingHistory, long> _bookingHistoryRepos;
        private readonly CloudMessagingManager _cloudMessagingManager;
        private readonly IRepository<FcmTokens, long> _fcmTokenRepos;
        private readonly IRepository<Citizen, long> _citizeRepos;

        public AdminTenantBusinessBookingAppService(
            IRepository<Booking, long> bookingRepos,
            IRepository<ItemBooking, long> itemBookingRepos,
            IRepository<ObjectPartner, long> storeBookingRepos,
            IRepository<LocalService, long> localServiceRepos,
            IOnlineClientManager onlineClientManager,
          //  ITenantBookingCommunicator TenantBookingCommunicator,
            IRepository<BookingHistory, long> bookingHistoryRepos,
            CloudMessagingManager cloudMessagingManager,
            IRepository<FcmTokens, long> fcmTokenRepos,
            IRepository<Citizen, long> citizeRepos
            )
        {
            _bookingRepos = bookingRepos;
            _itemBookingRepos = itemBookingRepos;
            _storeBookingRepos = storeBookingRepos;
            _onlineClientManager = onlineClientManager;
           // _tenantBookingCommunicator = TenantBookingCommunicator;
            _bookingHistoryRepos = bookingHistoryRepos;
            _cloudMessagingManager = cloudMessagingManager;
            _fcmTokenRepos = fcmTokenRepos;
            _localServiceRepos = localServiceRepos;
            _citizeRepos = citizeRepos;
        }


        #region booking
        public async Task<object> GetAllBookingAsync(GetBookingInput input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {

                    long t1 = TimeUtils.GetNanoseconds();
                    var query = (from bk in _bookingRepos.GetAll()
                                 join item in _itemBookingRepos.GetAll() on bk.ItemBookingId equals item.Id into tb_item
                                 from item in tb_item.DefaultIfEmpty()
                                 select new BookingDto()
                                 {
                                     Amount = bk.Amount,
                                     BookingTime = bk.BookingTime,
                                     CreationTime = bk.CreationTime,
                                     CreatorUserId = bk.CreatorUserId,
                                     CustomerAddress = bk.CustomerAddress,
                                     CustomerComment = bk.CustomerComment,
                                     CustomerName = bk.CustomerName,
                                     CustomerPhoneNumber = bk.CustomerPhoneNumber,
                                     Id = bk.Id,
                                     OrganizationUnitId = bk.OrganizationUnitId,
                                     ItemBookingId = bk.ItemBookingId,
                                     State = bk.State,
                                     StoreId = bk.StoreId,
                                     ItemBookingName = item.Name,
                                     TenantId = bk.TenantId,
                                     BookingDay = bk.BookingDay,
                                     BookingCode = bk.BookingCode,
                                     TypeBooking = bk.TypeBooking,
                                     CustomerInfo = (from ctz in _citizeRepos.GetAll()
                                                     where bk.CreatorUserId == ctz.AccountId
                                                     select ctz).FirstOrDefault()
                                 })
                                 .WhereIf(input.TypeBooking.HasValue, x => x.TypeBooking == input.TypeBooking)
                                 .Where(x => x.StoreId == input.StoreId)
                                 .WhereIf(input.FromTime.HasValue, u => u.BookingDay.HasValue && u.BookingDay >= input.FromTime)
                                 .WhereIf(input.ToTime.HasValue, u => u.BookingDay.HasValue && u.BookingDay <= input.ToTime)
                                 .WhereIf(input.ItemBookingId.HasValue, u => u.ItemBookingId == input.ItemBookingId)
                                 .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword), x => x.CustomerName.ToLower().Contains(input.Keyword.ToLower()) || x.CustomerPhoneNumber.ToLower().Contains(input.Keyword.ToLower()) || x.CustomerAddress.ToLower().Contains(input.Keyword.ToLower()) || x.BookingCode.ToLower().Contains(input.Keyword.ToLower()))
                                .OrderByDescending(x => x.CreationTime)
                                .AsQueryable();
                    switch (input.FormId)
                    {
                        case FormGetBooing.ACCEPTED:
                            query = query.Where(x => x.State == StateBooking.Accepted);
                            break;
                        case FormGetBooing.NEW:
                            query = query.Where(x => x.State == StateBooking.Requesting);
                            break;
                        case FormGetBooing.REFUSE:
                            query = query.Where(x => x.State == StateBooking.Denied);
                            break;
                        case FormGetBooing.CANCEL:
                            query = query.Where(x => x.State == StateBooking.Cancel);
                            break;
                        case FormGetBooing.EXPIRED:
                            query = query.Where(x => x.State == StateBooking.Expired);
                            break;
                        case FormGetBooing.COMPLETED:
                            query = query.Where(x => x.State == StateBooking.Completed);
                            break;
                        case FormGetBooing.GET_ALL:
                            break;
                        default:
                            query = query.Take(0);
                            break;
                    }
                    var day = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

                    if (input.TimeSelect.HasValue)
                    {
                        switch (input.TimeSelect.Value)
                        {
                            case FormTimeSelect.ALL:
                                break;
                            case FormTimeSelect.TODAY:

                                var headDay = new DateTime(day.Year, day.Month, day.Day);
                                var endDay = new DateTime(day.Year, day.Month, day.Day, 23, 59, 59);

                                query = query.Where(x => x.BookingDay.HasValue && x.BookingDay >= headDay && x.BookingDay < endDay);
                                break;
                            case FormTimeSelect.TOMORROW:

                                var headTDay = new DateTime(day.Year, day.Month, day.Day).AddDays(1);
                                var endTDay = new DateTime(day.Year, day.Month, day.Day, 23, 59, 59).AddDays(1);

                                query = query.Where(x => x.BookingDay.HasValue && x.BookingDay >= headTDay && x.BookingDay < endTDay);
                                break;
                            case FormTimeSelect.THISWEEK:
                                var dayOfWeek = day.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)day.DayOfWeek;
                                //var maxDay = DateTime.DaysInMonth(day.Year, day.Month);
                                var headWeek = day.AddDays(1 - dayOfWeek);

                                var endWeek = day.AddDays(7 - dayOfWeek);
                                query = query.Where(x => x.BookingDay.HasValue && x.BookingDay >= headWeek && x.BookingDay < endWeek);
                                break;
                            case FormTimeSelect.THISMONTH:

                                var headMonth = new DateTime(day.Year, day.Month, 1);
                                var endMonth = new DateTime(day.Year, day.Month, DateTime.DaysInMonth(day.Year,
                                                                day.Month), 23, 59, 59);
                                query = query.Where(x => x.BookingDay.HasValue && x.BookingDay >= headMonth && x.BookingDay < endMonth);
                                break;
                            default:
                                break;
                        }
                    }
                    var result = query.PageBy(input).ToList();
                    if (result != null && result.Count > 0)
                    {
                        foreach (var item in result)
                        {
                            var his = _bookingHistoryRepos.GetAll().Where(x => x.BookingId == item.Id && x.State == item.State).OrderBy(x => x.Id).LastOrDefault();
                            if (his != null)
                            {
                                item.RefuseReason = his.RefuseReason;
                                item.CancelReason = his.CancelReason;
                            }
                        }
                    }
                    var data = DataResult.ResultSuccess(result, "Get success", query.Count());
                    mb.statisticMetris(t1, 0, "gall_booking");
                    return data;
                }
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }

        }

      
        public Task<DataResult> UpdateStateBooking(UpdateBookingStateInput input)
        {
            try
            {
                var updateData = _bookingRepos.Get(input.BookingId);
                if (updateData != null)
                {
                    long t1 = TimeUtils.GetNanoseconds();

                    if (updateData != null && input.State != updateData.State)
                    {
                        updateData.State = input.State;
                        switch (input.State)
                        {
                            case StateBooking.Accepted:
                                updateData.BookingCode = GetUniqueKey(4) + updateData.Id;
                                HandBookingAccept(updateData);
                                break;
                            case StateBooking.Denied:
                                HandBookingDenied(updateData, input.RefuseReason);
                                break;
                            case StateBooking.Completed:
                                HandleBookingCompleted(updateData);
                                break;
                        }
                        _bookingRepos.Update(updateData);

                    }
                    mb.statisticMetris(t1, 0, "admin_ud_obj");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return Task.FromResult(data);
                }
                else
                {
                    var data = DataResult.ResultFail("Booking not found!");
                    return Task.FromResult(data);
                }

            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception");
                Logger.Fatal(e.Message);
                return Task.FromResult(data);
            }
        }

      
        public async Task<object> CreateOrUpdateBooking(BookingDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                input.TenantId = AbpSession.TenantId;
                if (input.Id > 0)
                {

                    var updateData = await _bookingRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);
                        await _bookingRepos.UpdateAsync(updateData);
                    }
                    mb.statisticMetris(t1, 0, "admin_ud_booking");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {
                    input.State = input.State == 0 ? StateBooking.Requesting : input.State;
                    var insertInput = input.MapTo<Booking>();
                    long id = await _bookingRepos.InsertAndGetIdAsync(insertInput);
                    mb.statisticMetris(t1, 0, "user_is_itembooking");
                    var data = DataResult.ResultSuccess(insertInput, "Insert success !");
                    return data;
                }

            }
            catch (Exception e)
            {
                throw;
            }
        }



        public async Task<object> DeleteBooking(long id)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                await _bookingRepos.DeleteAsync(id);
                var data = DataResult.ResultSuccess("Delete Success");
                mb.statisticMetris(t1, 0, "admin_del_booking");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> DeleteMultipleBooking([FromBody] List<long> ids)
        {
            try
            {

                if (ids.Count == 0) return DataResult.ResultError("Err", "input empty");
                var tasks = new List<Task>();
                foreach (var id in ids)
                {
                    var tk = _bookingRepos.DeleteAsync(id);
                    tasks.Add(tk);
                }
                Task.WaitAll(tasks.ToArray());

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

        public async Task<object> GetAllTimeBookedByItemAsync(GetTimeBookedByItemInput input)
        {
            try
            {
                var listTime = new List<string>();
                long t1 = TimeUtils.GetNanoseconds();
                var headDay = new DateTime(input.BookingDay.Year, input.BookingDay.Month, input.BookingDay.Day);
                var endDay = new DateTime(input.BookingDay.Year, input.BookingDay.Month, input.BookingDay.Day, 23, 59, 59);
                var times = _bookingRepos.GetAllList(x => x.ItemBookingId == input.ItemBookingId && (x.BookingDay.HasValue && x.BookingDay >= headDay && x.BookingDay < endDay) && (x.State == StateBooking.Requesting || x.State == StateBooking.Accepted));
                if (times != null && times.Count > 0)
                {
                    foreach (var time in times)
                    {
                        try
                        {
                            var arr = JsonConvert.DeserializeObject<List<string>>(time.BookingTime);
                            listTime.AddRange(arr);
                        }
                        catch { }
                    }
                    listTime = listTime.Distinct().ToList();
                }
                var data = DataResult.ResultSuccess(listTime, "Get success");
                mb.statisticMetris(t1, 0, "gall_booking");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }

        }

        #endregion

        public async Task<object> GetStoreNameByIdAsync(long id)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var result = await _storeBookingRepos.FirstOrDefaultAsync(id);
                if (result == null)
                {
                    return DataResult.ResultFail("Data not found !");
                }
                var localService = _localServiceRepos.FirstOrDefault(x => x.Type == result.Type.Value);
                var data = DataResult.ResultSuccess(new
                {
                    Name = result.Name,
                    TypeBooking = localService != null ? localService.TypeBooking : null
                }, "Get success");
                mb.statisticMetris(t1, 0, "gall_booking");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }

        }

        #region ItemBooking

        public async Task<object> GetAllItemBookingAsync(GetItemBookingInput input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var query = _itemBookingRepos.GetAll()
                    .Where(x => x.StoreId == input.StoreId)
                    .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword), x => x.Name.ToLower().Contains(input.Keyword.ToLower()))
                    .AsQueryable();
                var result = await query.PageBy(input).ToListAsync();
                var data = DataResult.ResultSuccess(result, "Get success", query.Count());
                mb.statisticMetris(t1, 0, "gall_booking");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }

        }

        public async Task<object> GetItemBookingByIdAsync(long id)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var result = await _itemBookingRepos.FirstOrDefaultAsync(id);
                var data = DataResult.ResultSuccess(result, "Get success");
                mb.statisticMetris(t1, 0, "get_one_booking");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }

        }

      
        public async Task<object> CreateOrUpdateItemBookingAsync(ItemBookingDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                if (input.IsFree.Value) input.Price = 0; 
                input.TenantId = AbpSession.TenantId;
                if (input.Id > 0)
                {
                    //update
                    var updateData = await _itemBookingRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);
                        await _itemBookingRepos.UpdateAsync(updateData);
                    }
                    mb.statisticMetris(t1, 0, "user_ud_itembooking");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {
                    var insertInput = input.MapTo<ItemBooking>();
                    long id = await _itemBookingRepos.InsertAndGetIdAsync(insertInput);
                    mb.statisticMetris(t1, 0, "user_is_itembooking");
                    var data = DataResult.ResultSuccess(insertInput, "Insert success !");
                    return data;
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<object> DeleteItemBooking(long id)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                await _itemBookingRepos.DeleteAsync(id);
                var data = DataResult.ResultSuccess("Delete Success");
                mb.statisticMetris(t1, 0, "admin_del_itembooking");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> DeleteMultipleItemBooking([FromBody] List<long> ids)
        {
            try
            {

                if (ids.Count == 0) return DataResult.ResultError("Err", "input empty");
                var tasks = new List<Task>();
                foreach (var id in ids)
                {
                    var tk = _itemBookingRepos.DeleteAsync(id);
                    tasks.Add(tk);
                }
                Task.WaitAll(tasks.ToArray());

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
        #endregion


        private Task HandBookingAccept(Booking booking)
        {
            var a = SendRealTimeNotifyStateBooking(booking);
            string message = $"Đơn đặt dịch vụ của bạn đã được chấp nhận. Mã xác nhận của bạn là : {booking.BookingCode}";
            var b = SendMessageNotify(message, booking.CreatorUserId.Value);
            //  Task.WaitAll(a, b);
            return Task.CompletedTask;

        }

        private Task SendMessageNotify(string message, long userId)
        {
            var devicesIds = _fcmTokenRepos.GetAllList(x => x.CreatorUserId == userId);
            var tokens = devicesIds.Select(x => x.Token).ToList();
            var a = _cloudMessagingManager.FcmSendToMultiDevice(new FcmMultiSendToDeviceInput()
            {
                Title = "Thông báo đơn đặt dịch vụ nội khu !",
                Body = message,
                Data = JsonConvert.SerializeObject(new
                {
                    action = "tenant_booking"
                }),
                Tokens = tokens
            });
            //   Task.WaitAll(a);
            return Task.CompletedTask;

        }

        private Task HandBookingDenied(Booking booking, string refuseReason)
        {
            var his = new BookingHistory()
            {
                BookingId = booking.Id,
                State = StateBooking.Denied,
                RefuseReason = refuseReason,
                TenantId = booking.TenantId
            };
            var a = _bookingHistoryRepos.InsertAsync(his);
            var b = SendRealTimeNotifyStateBooking(booking);
            string message = $"Thật tiếc đơn đặt dịch vụ của bạn đã bị từ chối. Nhấn để xem chi tiết ";
            var c = SendMessageNotify(message, booking.CreatorUserId.Value);
            //    Task.WaitAll(a, b, c);
            return Task.CompletedTask;
        }

        private async Task SendRealTimeNotifyStateBooking(Booking booking)
        {
            var users = await _onlineClientManager.GetAllByUserIdAsync(new UserIdentifier(booking.TenantId, booking.CreatorUserId.Value));
            if (users != null)
            {
             //   _tenantBookingCommunicator.SendNotifyTenantBusinessBookingToUser(users, (int)booking.State);
            }
        }

        private Task HandleBookingCompleted(Booking booking)
        {
            var b = SendRealTimeNotifyStateBooking(booking);
            string message = $"Đơn đặt dịch vụ của bạn đã hoàn thành. Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi !";
            var c = SendMessageNotify(message, booking.CreatorUserId.Value);
            //  Task.WaitAll( b, c);
            return Task.CompletedTask;
        }

    }
}
