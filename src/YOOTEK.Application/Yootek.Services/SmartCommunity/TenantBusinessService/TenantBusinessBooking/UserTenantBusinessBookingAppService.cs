using Abp;
using Abp.Application.Services;
using Abp.AutoMapper;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.RealTime;
using Abp.UI;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Yootek.Notifications;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Services
{
    public interface IUserTenantBusinessBookingAppService : IApplicationService
    {

    }

    public class UserTenantBusinessBookingAppService : YootekAppServiceBase, IUserTenantBusinessBookingAppService
    {
        private readonly IRepository<Booking, long> _bookingRepos;
        private readonly IRepository<ItemBooking, long> _itemBookingRepos;
        private readonly IRepository<ObjectPartner, long> _storeBookingRepos;
        private readonly IOnlineClientManager _onlineClientManager;
        private readonly IRepository<BookingHistory, long> _bookingHistoryRepos;
        private readonly IAppNotifier _appNotifier;
       // private readonly ITenantBookingCommunicator _tenantBookingCommunicator;

        public UserTenantBusinessBookingAppService(
            IRepository<Booking, long> bookingRepos,
            IRepository<ItemBooking, long> itemBookingRepos,
            IRepository<ObjectPartner, long> storeBookingRepos,
            IOnlineClientManager onlineClientManager,
            IRepository<BookingHistory, long> bookingHistoryRepos,
            IAppNotifier appNotifier
           // ITenantBookingCommunicator TenantBookingCommunicator
            )
        {
            _bookingRepos = bookingRepos;
            _itemBookingRepos = itemBookingRepos;
            _storeBookingRepos = storeBookingRepos;
            _onlineClientManager = onlineClientManager;
            _bookingHistoryRepos = bookingHistoryRepos;
            _appNotifier = appNotifier;
         //   _tenantBookingCommunicator = TenantBookingCommunicator;
        }


        #region booking
        public async Task<object> GetAllUserBookingAsync(GetBookingInput input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var query = (from bk in _bookingRepos.GetAll()
                             join item in _itemBookingRepos.GetAll() on bk.ItemBookingId equals item.Id into tb_item
                             from item in tb_item.DefaultIfEmpty()
                             join store in _storeBookingRepos.GetAll() on bk.StoreId equals store.Id into tb_store
                             from store in tb_store.DefaultIfEmpty()
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
                                 StoreName = store.Name,
                                 ItemBookingImageUrl = item.ImageUrl,
                                 BookingCode = bk.BookingCode,
                                 TypeBooking = bk.TypeBooking

                             })
                              .WhereIf(input.TypeBooking.HasValue, x => x.TypeBooking == input.TypeBooking)
                    .Where(x => x.CreatorUserId == AbpSession.UserId)
                    .WhereIf(input.StoreId > 0, x => x.StoreId == input.StoreId)
                    .OrderByDescending(x => x.CreationTime)
                    .AsQueryable();
                switch (input.FormId)
                {
                    case FormGetBooing.USER_GET_ACCEPTED:
                        query = query.Where(x => x.State == StateBooking.Accepted);
                        break;
                    case FormGetBooing.USER_GET_REQUESTING:
                        query = query.Where(x => x.State == StateBooking.Requesting);
                        break;
                    case FormGetBooing.USER_GET_COMPLETED:
                        query = query.Where(x => x.State == StateBooking.Completed);
                        break;
                    case FormGetBooing.USER_GET_CANCEL:
                        query = query.Where(x => x.State == StateBooking.Denied || x.State == StateBooking.Cancel || x.State == StateBooking.Expired);
                        break;
                    case FormGetBooing.USER_GETALL:
                        break;
                    default:
                        query = query.Take(0);
                        break;
                }
                var result = await query.PageBy(input).ToListAsync();
                if (result != null && result.Count > 0)
                {
                    foreach (var item in result)
                    {
                        var his = _bookingHistoryRepos.GetAll().Where(x => x.BookingId == item.Id && x.State == item.State).OrderBy(x => x.Id).LastOrDefault();
                        if (his != null)
                        {
                            item.RefuseReason = his.RefuseReason;
                            item.CancelReason = his.CancelReason;
                            item.TimeRefuseOrCancel = his.CreationTime;
                        }
                    }
                }
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

      
        public async Task<object> CreateOrUpdateBooking(BookingDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                input.TenantId = AbpSession.TenantId;
                if (!CheckStateBooking(input.State)) return DataResult.ResultFail("State booking is invalid !");
                //if (input.BookingDay.HasValue && input.State != StateBooking.Cancel  && !CheckBookingDay(input.BookingDay.Value)) return DataResult.ResultFail("Booking day is invalid !");
                if (input.Id > 0)
                {

                    var updateData = await _bookingRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        updateData.State = input.State;
                        await _bookingRepos.UpdateAsync(updateData);

                        if (updateData.State == StateBooking.Cancel)
                        {
                            var his = new BookingHistory()
                            {
                                BookingId = updateData.Id,
                                State = updateData.State,
                                CancelReason = input.CancelReason,
                                TenantId = input.TenantId
                            };
                            await _bookingHistoryRepos.InsertAsync(his);
                        }
                    }
                   // await _tenantBookingCommunicator.SendNotifyTenantBusinessBookingToAdmin(updateData);
                    mb.statisticMetris(t1, 0, "admin_ud_booking");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {
                    var store = await _storeBookingRepos.FirstOrDefaultAsync(input.StoreId);
                    if(store == null) return DataResult.ResultSuccess( "Store not found !");

                    input.State = StateBooking.Requesting;
                    var insertInput = ObjectMapper.Map<Booking>(input);
                    long id = await _bookingRepos.InsertAndGetIdAsync(insertInput);

                    var admins = await UserManager.GetUserOrganizationUnitByUrban(store.UrbanId ?? 0);
                    await NotifierNewBooking(insertInput, admins.ToArray());
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

        #endregion


        #region ItemBooking

        public async Task<object> GetAllItemBookingAsync(GetItemBookingInput input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var query = (from it in _itemBookingRepos.GetAll()
                             select new ItemBookingDto()
                             {
                                 Description = it.Description,
                                 ImageUrl = it.ImageUrl,
                                 Name = it.Name,
                                 Price = it.Price,
                                 StoreId = it.StoreId,
                                 Unit = it.Unit,
                                 OpenTimes = it.OpenTimes,
                                 Id = it.Id,
                                 IsFree = it.IsFree
                             }).AsQueryable();
                var result = await query.Where(x => x.StoreId == input.StoreId).PageBy(input).ToListAsync();
                var data = DataResult.ResultSuccess(result, "Get success");
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


        #region Common
        private List<int> ConvertTimeItemBooking(ItemBooking item)
        {
            switch (item.Unit)
            {
                case UnitItemBooking.HalfHour:
                    break;
                case UnitItemBooking.OneHour:
                    break;
                case UnitItemBooking.OneDay:
                    break;
                default:
                    break;
            }
            return null;
        }

        private bool CheckStateBooking(StateBooking state)
        {
            if (state == StateBooking.Requesting || state == StateBooking.Cancel) return true;
            return false;

        }

        private async Task NotifierNewBooking(Booking booking, UserIdentifier[] admin)
        {
            try
            {
                var detailUrlApp = $"yooioc://app/amenities-booking/detail?id={booking.ItemBookingId}";
                var detailUrlWA = $"/amenities-booking/detail?id={booking.ItemBookingId}";
                var messageDeclined = new UserMessageNotificationDataBase(
                AppNotificationAction.CitizenVerify,
                AppNotificationIcon.TenantBusinessIcon,
                TypeAction.Detail,
                $"Bạn có một đơn đặt tiện ích nội khu mới. Nhấn để xem chi tiết !",
                detailUrlApp,
                detailUrlWA);
                await _appNotifier.SendMessageNotificationInternalAsync(
                    "Yoolife tiện ích nội khu!",
                       $"Bạn có một đơn đặt tiện ích nội khu mới. Nhấn để xem chi tiết !",
                     detailUrlApp,
                     detailUrlWA,
                    admin.ToArray(),
                    messageDeclined,
                    AppType.IOC);
            }
            catch { }
        }

        #endregion
    }
}
