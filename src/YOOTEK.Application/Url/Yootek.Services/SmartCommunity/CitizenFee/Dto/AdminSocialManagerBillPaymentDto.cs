using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yootek.Common.Enum;
using Yootek.Common;
using Yootek.Services.Dto;

namespace YOOTEK.Yootek.Services.SmartCommunity.CitizenFee.Dto
{
    public class GetAllBillPaymentByAdminSocialDto : CommonInputDto
    {
        [CanBeNull] public DateTime? Period { get; set; }
        public UserBillPaymentStatus? Status { get; set; }
        public UserBillPaymentMethod? Method { get; set; }
        public string ApartmentCode { get; set; }
        public bool IsAdvanced { get; set; }
        public int? TenantId { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public DateTime? FromDay { get; set; }
        public DateTime? ToDay { get; set; }
        public DateTime? InDay { get; set; }
    }
}
