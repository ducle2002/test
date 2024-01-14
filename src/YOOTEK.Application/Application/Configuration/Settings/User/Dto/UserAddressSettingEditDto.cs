using Yootek.App.ServiceHttpClient.Dto.VietNamAdministrativeUnit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Application.Configuration.Settings.User.Dto
{
    public class UserAddressSettingEditDto
    {
        public string Address { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string ProvinceCode { get; set; }
        public string DistrictCode { get; set; }
        public string WardCode { get; set; }

        public Province Province { get; set; }
        public District District { get; set; }
        public Ward Ward { get; set; }
    }
}
