namespace Yootek.Yootek.Services.Yootek.DichVu.Payment
{
    public class IPNPaymentMomoInputDto
    {
        public static string OrderTypeDefault = "momo_wallet";
        public string PartnerCode { get; set; }
        public string OrderId { get; set; }
        public string RequestId { get; set; }
        public double Amount { get; set; }
        public string OrderInfo { get; set; }
        public string OrderType { get; set; }
        public long TransId { get; set; }
        public int ResultCode { get; set; }
        public string Message { get; set; }
        public string PayType { get; set; }
        public long ResponseTime { get; set; }
        public string ExtraData { get; set; }
        public string Signature { get; set; }

        public override string ToString()
        {
            return base.ToString() + ", " + "amount: " + Amount + ", orderId: " + OrderId + ", requestId: " + RequestId + ", orderInfo: " + OrderInfo + ", orderType: " + OrderType + ", transId: " + TransId + ", resultCode: " + ResultCode + ", message: " + Message + ", payType: " + PayType + ", responseTime: " + ResponseTime + ", extraData: " + ExtraData + ", signature: " + Signature;
        }
    }
}
