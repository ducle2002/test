using System;
using Yootek.Common;

namespace Yootek.Services
{
    public class GetAllRecoverPaymentHistoryInput : CommonInputDto
    {
        public int? Method { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public string ApartmentCode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ToDay { get; set; }
        public DateTime EndDate { get; set; }
    }
}
