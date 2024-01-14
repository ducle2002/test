using Yootek.App.ServiceHttpClient;
using System;
using System.Threading.Tasks;

namespace Yootek.Yootek.Services.Yootek.SmartSocial.VietnameseAdministrative
{
    public interface IVietnameseAdministrativeAppService
    {
        Task<object> GetAllProvinces();
        Task<object> GetAllDistricts(string provinceCode);
        Task<object> GetProvinceByCode(string provinceCode);
        Task<object> GetDistrictByCode(string districtCode);
        Task<object> GetAllWards(string districtCode);
        Task<object> GetWardByCode(string wardCode);

    }
    public class VietnameseAdministrativeAppService : YootekAppServiceBase, IVietnameseAdministrativeAppService
    {
        private readonly IVietNamAdministrativeUnitService _vietNamAdministrativeUnitService;
        public VietnameseAdministrativeAppService(IVietNamAdministrativeUnitService vietNamAdministrativeUnitService)
        {
            _vietNamAdministrativeUnitService = vietNamAdministrativeUnitService;
        }
        public async Task<object> GetAllProvinces()
        {
            try
            {
                var data = await _vietNamAdministrativeUnitService.GetProvinces();
                return data;
            } catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw new Exception(ex.Message);
            }
        }
        public async Task<object> GetAllDistricts(string provinceCode)
        {
            try
            {
                var data = await _vietNamAdministrativeUnitService.GetDistricts(provinceCode);
                return data;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw new Exception(ex.Message);
            }
        }
        public async Task<object> GetProvinceByCode(string provinceCode)
        {
            try
            {
                var data = await _vietNamAdministrativeUnitService.GetProvinceByCode(provinceCode);
                return data;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw new Exception(ex.Message);
            }
        }
        public async Task<object> GetDistrictByCode(string districtCode)
        {
            try
            {
                var data = await _vietNamAdministrativeUnitService.GetDistrictByCode(districtCode);
                return data;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw new Exception(ex.Message);
            }
        }
        public async Task<object> GetAllWards(string districtCode)
        {
            try
            {
                var data = await _vietNamAdministrativeUnitService.GetWards(districtCode);
                return data;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw new Exception(ex.Message);
            }
        }
        public async Task<object> GetWardByCode(string wardCode)
        {
            try
            {
                var data = await _vietNamAdministrativeUnitService.GetWardByCode(wardCode);
                return data;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw new Exception(ex.Message);
            }
        }
    }
}
