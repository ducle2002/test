using Abp.AutoMapper;
using Yootek.Yootek.EntityDb.Smarthome.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Service.Dto
{
    [AutoMap(typeof(HomeGateway))]
    public class HomeGatewayDto : HomeGateway
    {
    }




}
