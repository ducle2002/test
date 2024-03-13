using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yootek.Common;
using Yootek.Services;

namespace YOOTEK.Yootek.Services
{
    public class GetAllDigitalServicePaymentInput: CommonInputDto
    {
        public long? UrbanId { get; set; }  
        public long? BuildingId {  get; set; }
        public long? ServiceId { get; set; }
    }

    public class HandPaymentForThirdPartyInput
    {
        public int Id { get; set; }
        public EPrepaymentStatus Status { get; set; }
        public int? TenantId { get; set; }
    }
}
