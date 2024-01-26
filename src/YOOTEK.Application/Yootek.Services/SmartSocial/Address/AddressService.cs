using Abp;
using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.UI;
using Yootek.ApbCore.Data;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Yootek.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Yootek.Services.Yootek.DichVu.Address
{
    public interface IAddressService : IApplicationService
    {
        Task<object> GetAllProvinceAsync();
        Task<object> GetAllDistrictWithProvinceAsync(DistrictInputDto input);
        Task<object> GetAllWardWithDistrictAsync(WardInputDto input);
    }

    public class AddressService : YootekAppServiceBase, IAddressService
    {

        private readonly IRepository<Province, string> _provinceRepos;
        private readonly IRepository<District, string> _districtRepos;
        private readonly IRepository<Ward, string> _wardRepos;
        private readonly ISqlExecuter _sqlExecute;


        public AddressService(
            IRepository<Province, string> provinceRepos, IRepository<District, string> districtRepos, IRepository<Ward, string> wardRepos
            )
        {
            _provinceRepos = provinceRepos;
            _districtRepos = districtRepos;
            _wardRepos = wardRepos;
        }

        public async Task<object> GetAllDistrictWithProvinceAsync(DistrictInputDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var result = await _districtRepos.GetAllListAsync(x => x.ProvinceId == input.ProvinceId);

                var data = DataResult.ResultSuccess(result, "Get success");
                mb.statisticMetris(t1, 0, "gall_district");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> GetAllProvinceAsync()
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var result = await _provinceRepos.GetAllListAsync();

                var data = DataResult.ResultSuccess(result, "Get success");
                mb.statisticMetris(t1, 0, "gall_province");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> GetAllWardWithDistrictAsync(WardInputDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var result = await _wardRepos.GetAllListAsync(x => x.DistrictId == input.DistrictId);

                var data = DataResult.ResultSuccess(result, "Get success");
                mb.statisticMetris(t1, 0, "gall_ward");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }
    }
}
