namespace Yootek.Yootek.Services.Yootek.Payments.Dto
{
    public class OnepayIpnInputDto
    {
        public string Vpc_TokenNum { get; set; }
        public string Vpc_TokenExp { get; set; }
        public string Vpc_Command { get; set; }
        public string Vpc_Locale { get; set; }
        public string Vpc_CurrencyCode { get; set; }
        public string Vpc_MerchTxnRef { get; set; }
        public string Vpc_Merchant { get; set; }
        public string Vpc_OrderInfo { get; set; }
        public string Vpc_Amount { get; set; }
        public string Vpc_TxnResponseCode { get; set; }
        public string Vpc_TransactionNo { get; set; }
        public string Vpc_Message { get; set; }
        public string Vpc_Card { get; set; }
        public string Vpc_PayChannel { get; set; }
        public string Vpc_CardUid { get; set; }
        public string Vpc_SecureHash { get; set; }
    }
}