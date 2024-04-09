using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yootek.Common;
using Yootek.Services;
using Yootek.Yootek.Services.Yootek.SmartCommunity.CitizenFee.Dto;
using YOOTEK.EntityDb;

namespace YOOTEK.Yootek.Services
{
    public class GetAllDigitalServicePaymentInput: CommonInputDto
    {
        public int TenantId { get; set; }
        public long? UrbanId { get; set; }  
        public long? BuildingId {  get; set; }
        public long? ServiceId { get; set; }
    }

    public class UserGetAllDigitalServicePaymentInput : CommonInputDto
    {
    }


    public class HandPaymentDigitalServiceForThirdPartyInput
    {
        public int Id { get; set; }
        public EPrepaymentStatus Status { get; set; }
        public int? TenantId { get; set; }
    }

    public class RequestPaymentDigitalServiceInput
    {
        public int? TenantId { get; set; }
        public long TransactionId { get; set; } // orderId, bookingId, ...
        public long Amount { get; set; }
    }
}
