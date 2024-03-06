using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yootek.Common.Enum;
using Yootek.Common;
using Yootek.Services.Dto;

namespace YOOTEK.Yootek.Services.SmartCommunity.CitizenFee.ApartmentBalanceBill
{
    public class GetApartmentBalanceInput : CommonInputDto
    {
        public string ApartmentCode { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public DateTime? FromDay { get; set; }
        public DateTime? ToDay { get; set; }
    }

    public class GetTotalApartmentBalanceInput
    {
        public string ApartmentCode { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public bool IsWhereByType { get; set; }
        public BillType? BillType { get; set; }

    }
}
