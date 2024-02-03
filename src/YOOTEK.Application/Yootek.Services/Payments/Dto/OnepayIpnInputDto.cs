namespace Yootek.Yootek.Services.Yootek.Payments.Dto
{
    public class OnepayIpnInputDto
    {
        public string vpc_TokenNum { get; set; }
        public string vpc_TokenExp { get; set; }
        public string vpc_Command { get; set; }
        public string vpc_Locale { get; set; }
        public string vpc_CurrencyCode { get; set; }
        public string vpc_MerchTxnRef { get; set; }
        public string vpc_Merchant { get; set; }
        public string vpc_OrderInfo { get; set; }
        public string vpc_Amount { get; set; }
        public string vpc_TxnResponseCode { get; set; }
        public string vpc_TransactionNo { get; set; }
        public string vpc_Message { get; set; }
        public string vpc_Card { get; set; }
        public string vpc_PayChannel { get; set; }
        public string vpc_CardUid { get; set; }
        public string vpc_SecureHash { get; set; }
    }
}