using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.Organizations;
using Abp.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yootek.Application;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Yootek.Yootek.Services.Yootek.SmartCommunity.Guests.Dto;

namespace Yootek.Yootek.Services.Yootek.SmartCommunity.Guests
{
    public class GuestAppService : YootekAppServiceBase
    {
        private readonly IRepository<Guest, long> _guestRepo;
        private readonly IRepository<GuestForm, long> _guestFormRepo;
        private readonly IRepository<OrganizationUnit, long> _ouRepo;

        public GuestAppService(IRepository<Guest, long> guestRepo, IRepository<OrganizationUnit, long> ouRepo,
            IRepository<GuestForm, long> guestFormRepo)
        {
            _guestRepo = guestRepo;
            _ouRepo = ouRepo;
            _guestFormRepo = guestFormRepo;
        }

        [HttpGet]
        public async Task<DataResult> AdminGetListGuest(GetListGuestDto input)
        {
            try
            {
                var query = _guestRepo.GetAll().Select(g => new BasicGuestDto()
                {
                    Id = g.Id,
                    TenantId = g.TenantId,
                    Name = g.Name,
                    PhoneNumber = g.PhoneNumber,
                    CheckInTime = g.CheckInTime,
                    CheckOutTime = g.CheckOutTime,
                    Status = g.Status,
                    GuestFormId = g.GuestFormId,
                    GuestFormName = _guestFormRepo.GetAll().Where(gf => gf.Id == g.GuestFormId)
                            .Select(gf => gf.Name)
                            .FirstOrDefault(),
                    UrbanId = _guestFormRepo.GetAll().Where(gf => gf.Id == g.GuestFormId)
                            .Select(gf => gf.UrbanId)
                            .FirstOrDefault(),
                    UrbanName = _ouRepo.GetAll().Where(o => o.Id == _guestFormRepo.GetAll()
                            .Where(gf => gf.Id == g.GuestFormId)
                            .Select(gf => gf.UrbanId)
                            .FirstOrDefault()).Select(o => o.DisplayName)
                            .FirstOrDefault(),
                    CreationTime = g.CreationTime,
                    PlaceDetail = g.PlaceDetail,
                    HostName = g.HostName,
                })
                    .WhereIf(input.UrbanId.HasValue, x => x.UrbanId == input.UrbanId)
                    .WhereIf(input.Status.HasValue, x => x.Status == input.Status)
                    .WhereIf(!input.Keyword.IsNullOrEmpty(),
                        x => x.Name.ToLower().Contains(input.Keyword.ToLower()) ||
                             x.PhoneNumber.Contains(input.Keyword))
                    .WhereIf(input.CheckInTimeFrom.HasValue,
                        x => x.CheckInTime >= input.CheckInTimeFrom.Value)
                    .WhereIf(input.CheckInTimeTo.HasValue,
                        x => x.CheckInTime <= input.CheckInTimeTo.Value)
                    .WhereIf(input.CheckOutTimeFrom.HasValue,
                        x => x.CheckOutTime >= input.CheckOutTimeFrom.Value)
                    .WhereIf(input.CheckOutTimeTo.HasValue,
                        x => x.CheckOutTime <= input.CheckOutTimeTo.Value)
                    .WhereIf(input.CreationDate.HasValue,
                        x => x.CreationTime.Date == input.CreationDate.Value.Date)
                    .WhereIf(input.CreationTimeFrom.HasValue,
                        x => x.CreationTime >= input.CreationTimeFrom.Value)
                    .WhereIf(input.CreationTimeTo.HasValue,
                        x => x.CreationTime <= input.CreationTimeTo.Value);

                var totalCount = query.Count();
                var result = await query
                    .ApplySort(input.OrderBy, input.SortBy)
                    .ApplySort(GetListGuestOrderBy.CreationTime)
                    .Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();

                return DataResult.ResultSuccess(result, "Get success", totalCount);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        [HttpGet]
        public async Task<DataResult> AdminGetDetailGuest(GetDetailGuestDto input)
        {
            try
            {
                var result = await _guestRepo.GetAll().Select(g => new DetailGuestDo()
                {
                    Id = g.Id,
                    TenantId = g.TenantId,
                    Name = g.Name,
                    PhoneNumber = g.PhoneNumber,
                    Address = g.Address,
                    IdNumber = g.IdNumber,
                    IdCardFrontImageUrl = g.IdCardFrontImageUrl,
                    IdCardBackImageUrl = g.IdCardBackImageUrl,
                    CheckInTime = g.CheckInTime,
                    CheckOutTime = g.CheckOutTime,
                    Status = g.Status,
                    Vehicle = g.Vehicle,
                    VehicleNumber = g.VehicleNumber,
                    GuestFormId = g.GuestFormId,
                    GuestFormName = _guestFormRepo.GetAll().Where(gf => gf.Id == g.GuestFormId)
                            .Select(gf => gf.Name)
                            .FirstOrDefault(),
                    UrbanId = _guestFormRepo.GetAll().Where(gf => gf.Id == g.GuestFormId)
                            .Select(gf => gf.UrbanId)
                            .FirstOrDefault(),
                    UrbanName = _ouRepo.GetAll().Where(o => o.Id == _guestFormRepo.GetAll()
                            .Where(gf => gf.Id == g.GuestFormId)
                            .Select(gf => gf.UrbanId)
                            .FirstOrDefault()).Select(o => o.DisplayName)
                            .FirstOrDefault(),
                    HostName = g.HostName,
                    HostPhoneNumber = g.HostPhoneNumber,
                    Description = g.Description,
                    CreationTime = g.CreationTime,
                    PlaceDetail = g.PlaceDetail,
                })
                    .Where(x => x.Id == input.Id)
                    .ToListAsync();

                if (!result.Any())
                {
                    throw new UserFriendlyException("Guest not found");
                }

                return DataResult.ResultSuccess(result.First(), "Get success");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        [HttpPost]
        public async Task<DataResult> UserSubmitForm(UserSubmitFormDto input)
        {
            try
            {
                var guest = input.MapTo<Guest>();
                guest.Status = GuestStatus.Pending;

                guest.CheckInTime.AddHours(-7);
                if (guest.CheckOutTime.HasValue) guest.CheckOutTime.Value.AddHours(-7);

                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
                {
                    var guestForm = await _guestFormRepo.GetAsync(input.GuestFormId);
                    if (guestForm == null)
                    {
                        throw new UserFriendlyException("Guest form not found");
                    }

                    guest.TenantId = guestForm.TenantId;

                    await _guestRepo.InsertAsync(guest);

                    await CurrentUnitOfWork.SaveChangesAsync();
                }

                return DataResult.ResultSuccess(guest, "Submit success");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        [HttpPost]
        public async Task<DataResult> AdminChangeStatus(AdminChangeStatusDto input)
        {
            try
            {
                var guest = await _guestRepo.GetAsync(input.Id);

                if (guest.Status != GuestStatus.Pending && guest.Status != GuestStatus.Rejected &&
                    guest.Status != GuestStatus.Approved)
                {
                    throw new UserFriendlyException("Guest status is not available to change");
                }

                guest.Status = input.Status;

                await _guestRepo.UpdateAsync(guest);

                await CurrentUnitOfWork.SaveChangesAsync();

                return DataResult.ResultSuccess(guest, "Change status success");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
    }
}