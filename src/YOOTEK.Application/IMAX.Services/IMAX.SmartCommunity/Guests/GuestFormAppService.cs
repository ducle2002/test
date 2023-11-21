using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Organizations;
using Abp.UI;
using IMAX.Common.DataResult;
using IMAX.EntityDb;
using IMAX.IMAX.Services.IMAX.SmartCommunity.Guests.Dto;
using Microsoft.AspNetCore.Mvc;
using Abp.Linq.Extensions;
using IMAX.Application;
using Microsoft.EntityFrameworkCore;

namespace IMAX.IMAX.Services.IMAX.SmartCommunity.Guests
{
    public class GuestFormAppService : IMAXAppServiceBase
    {
        private readonly IRepository<GuestForm, long> _guestFormRepo;
        private readonly IRepository<OrganizationUnit, long> _ouRepo;

        public GuestFormAppService(IRepository<OrganizationUnit, long> ouRepo,
            IRepository<GuestForm, long> guestFormRepo)
        {
            _ouRepo = ouRepo;
            _guestFormRepo = guestFormRepo;
        }

        [HttpPost]
        public async Task<DataResult> AdminCreateGuestForm(CreateGuestFormDto input)
        {
            try
            {
                var urban = await _ouRepo.GetAsync(input.UrbanId);
                if (urban == null)
                {
                    throw new UserFriendlyException("Urban not found");
                }

                var guestForm = input.MapTo<GuestForm>();

                await _guestFormRepo.InsertAsync(guestForm);

                await CurrentUnitOfWork.SaveChangesAsync();

                return DataResult.ResultSuccess(guestForm, "Create successfully");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }

        [HttpGet]
        public async Task<DataResult> AdminGetListGuestForm(GetListGuestFormDto input)
        {
            try
            {
                var query = _guestFormRepo.GetAll().Select(gf => new BasicGuestFormDto()
                    {
                        Id = gf.Id,
                        TenantId = gf.TenantId,
                        ImageUrl = gf.ImageUrl,
                        UrbanId = gf.UrbanId,
                        UrbanName = _ouRepo.GetAll().Where(o => o.Id == gf.UrbanId).Select(o => o.DisplayName)
                            .FirstOrDefault(),
                        Name = gf.Name,
                        PhoneNumber = gf.PhoneNumber,
                        Address = gf.Address,
                        Description = gf.Description,
                        CreationTime = gf.CreationTime
                    })
                    .WhereIf(input.UrbanId.HasValue, x => x.UrbanId == input.UrbanId);

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
                throw new UserFriendlyException(e.Message);
            }
        }

        [HttpGet]
        public async Task<DataResult> AdminGetDetailGuestForm(long id)
        {
            try
            {
                var result = await _guestFormRepo.GetAll().Select(gf => new DetailGuestFormDto()
                    {
                        Id = gf.Id,
                        TenantId = gf.TenantId,
                        ImageUrl = gf.ImageUrl,
                        UrbanId = gf.UrbanId,
                        UrbanName = _ouRepo.GetAll().Where(o => o.Id == gf.UrbanId).Select(o => o.DisplayName)
                            .FirstOrDefault(),
                        Name = gf.Name,
                        PhoneNumber = gf.PhoneNumber,
                        Address = gf.Address,
                        Description = gf.Description,
                        CreationTime = gf.CreationTime
                    })
                    .Where(gf => gf.Id == id)
                    .ToListAsync();

                return DataResult.ResultSuccess(result.FirstOrDefault(), "Get success");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }

        [HttpPut]
        public async Task<DataResult> AdminUpdateGuestForm(UpdateGuestFormDto input)
        {
            try
            {
                var guestForm = await _guestFormRepo.GetAsync(input.Id);

                if (guestForm == null)
                {
                    throw new UserFriendlyException("Guest form not found");
                }

                guestForm.Name = input.Name ?? guestForm.Name;
                guestForm.PhoneNumber = input.PhoneNumber ?? guestForm.PhoneNumber;
                guestForm.Address = input.Address ?? guestForm.Address;
                guestForm.Description = input.Description ?? guestForm.Description;

                await _guestFormRepo.UpdateAsync(guestForm);

                await CurrentUnitOfWork.SaveChangesAsync();

                return DataResult.ResultSuccess(guestForm, "Update successfully");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }

        [HttpDelete]
        public async Task<DataResult> AdminDeleteGuestForm(long id)
        {
            try
            {
                var guestForm = await _guestFormRepo.GetAsync(id);

                if (guestForm == null)
                {
                    throw new UserFriendlyException("Guest form not found");
                }

                await _guestFormRepo.DeleteAsync(guestForm);

                await CurrentUnitOfWork.SaveChangesAsync();

                return DataResult.ResultSuccess(guestForm, "Delete successfully");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }

        [HttpGet]
        public async Task<DataResult> UserGetGuestForm(long id)
        {
            try
            {
                var result = await _guestFormRepo.GetAll().Select(gf => new DetailGuestFormDto()
                    {
                        Id = gf.Id,
                        TenantId = gf.TenantId,
                        ImageUrl = gf.ImageUrl,
                        UrbanId = gf.UrbanId,
                        UrbanName = _ouRepo.GetAll().Where(o => o.Id == gf.UrbanId).Select(o => o.DisplayName)
                            .FirstOrDefault(),
                        Name = gf.Name,
                        PhoneNumber = gf.PhoneNumber,
                        Address = gf.Address,
                        Description = gf.Description,
                        CreationTime = gf.CreationTime
                    })
                    .Where(gf => gf.Id == id)
                    .ToListAsync();

                return DataResult.ResultSuccess(result.FirstOrDefault(), "Get success");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }
    }
}