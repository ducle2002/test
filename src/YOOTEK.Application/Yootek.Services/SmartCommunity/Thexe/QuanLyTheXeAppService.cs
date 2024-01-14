using Abp;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Organizations;
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

namespace Yootek.Yootek.Services.Namcuong.Dothi
{
    public interface IQuanLyTheXeService : IApplicationService
    {
    }

    [AbpAuthorize]
    public class QuanLyTheXeAppService : YootekAppServiceBase, IQuanLyTheXeService
    {
        private readonly IRepository<TheXe, long> _theXeRepos;

        public QuanLyTheXeAppService(IRepository<TheXe, long> theXeRepos)
        {
            _theXeRepos = theXeRepos;
        }

        public async Task<object> GetAllTheXe(GetAllTheXeInput input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var data = _theXeRepos.GetAll()
                        .WhereIf(!string.IsNullOrEmpty(input.Keyword),
                            x => x.ApartmentCode.Contains(input.Keyword)
                            || x.ApartmentOwnerName.Contains(input.Keyword)
                            || x.Code.Contains(input.Keyword)
                            || x.LicensePlate.Contains(input.Keyword)
                            || x.PhoneNumber.Contains(input.Keyword)
                            || x.BuildingCode.Contains(input.Keyword))
                        .WhereIf(input.FromDay.HasValue, x => x.CreationTime >= input.FromDay)
                        .WhereIf(input.ToDay.HasValue, x => x.CreationTime <= input.ToDay)
                        .WhereIf(input.Status.HasValue, x => x.Status == input.Status);
                    var paginatedData = await data.PageBy(input).ToListAsync();

                    return DataResult.ResultSuccess(paginatedData, "Get success!", data.Count());
                }
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception!");
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }

        public async Task<object> CreateOrUpdateTheXe(TheXeInputDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                input.TenantId = AbpSession.TenantId;
                if (input.Id > 0)
                {
                    var updateData = await _theXeRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);

                        await _theXeRepos.UpdateAsync(updateData);
                    }
                    mb.statisticMetris(t1, 0, "ud_thexe");

                    var data = DataResult.ResultSuccess(updateData, "Update success!");
                    return data;
                }
                else
                {
                    var insertInput = input.MapTo<TheXe>();
                    insertInput.Status = TheXeStatus.Active;

                    long id = await _theXeRepos.InsertAndGetIdAsync(insertInput);
                    insertInput.Id = id;

                    mb.statisticMetris(t1, 0, "is_thexe");

                    var data = DataResult.ResultSuccess(insertInput, "Insert success!");
                    return data;
                }

            }
            catch (Exception e)
            {
                Logger.Info(e.ToString());

                var data = DataResult.ResultError(e.ToString(), "Exception!");
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }

        public async Task<object> DeleteTheXe(long id)
        {
            try
            {
                var deleteData = await _theXeRepos.GetAsync(id);
                if (deleteData != null)
                {
                    await _theXeRepos.DeleteAsync(deleteData);
                }
                return null;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception!");
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }
    }
}
