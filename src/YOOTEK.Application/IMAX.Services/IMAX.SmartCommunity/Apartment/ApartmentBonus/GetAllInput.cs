using IMAX.Common;
using IMAX.EntityDb;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMAX.IMAX.Services.IMAX.SmartCommunity.ApartmentDiscount
{
    public class GetAllInput : CommonInputDto
    {
    }

    public class GetAllByApartmentCodeInput 
    {
        public string ApartmentCode { get; set; }
    }
}
