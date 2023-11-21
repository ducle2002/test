using Abp.AutoMapper;
using IMAX.IMAX.EntityDb.Smarthome.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMAX.Service.Dto
{
    [AutoMap(typeof(HomeGateway))]
    public class HomeGatewayDto : HomeGateway
    {
    }




}
