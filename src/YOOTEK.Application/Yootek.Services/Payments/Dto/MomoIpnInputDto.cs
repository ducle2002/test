namespace Yootek.Yootek.Services.Yootek.Payments.Dto
{
    public class MomoIpnInputDto
    {
        public int Amount { get; set; }
        public string ExtraData { get; set; }
        public string Message { get; set; }
        public string OrderId { get; set; }
        public string OrderType { get; set; } = "momo_wallet";
        public string OrderInfo { get; set; }
        public string PartnerCode { get; set; }
        public string PayType { get; set; }
        public string RequestId { get; set; }
        public int ResponseTime { get; set; }
        public int ResultCode { get; set; }
        public int TransId { get; set; }
        public string Signature { get; set; }
    }
}