using Yootek.Common;
using Yootek.EntityDb;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Yootek.Services.Yootek.SmartCommunity.ApartmentDiscount
{
    public class GetAllInput : CommonInputDto
    {
    }

    public class GetAllByApartmentCodeInput 
    {
        public string ApartmentCode { get; set; }
    }
}
