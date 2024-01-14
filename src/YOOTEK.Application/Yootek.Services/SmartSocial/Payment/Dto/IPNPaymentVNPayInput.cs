using System.Text.Json.Serialization;

namespace Yootek.Yootek.Services.Yootek.DichVu.Payment
{
    public class IPNPaymentVNPayInputDto
    {
        public string vnp_TmnCode { get; set; }
        public int vnp_Amount { get; set; }
        public string vnp_BankCode { get; set; }
        public string vnp_BankTranNo { get; set; }
        public string vnp_CardType { get; set; }
        public string vnp_PayDate { get; set; }
        public string vnp_OrderInfo { get; set; }
        public string vnp_TransactionNo { get; set; }
        public string vnp_ResponseCode { get; set; }
        public string vnp_TransactionStatus { get; set; }
        public string vnp_TxnRef { get; set; }
        public string vnp_SecureHashType { get; set; }
        public string vnp_SecureHash { get; set; }


        public override string ToString()
        {
            return base.ToString() + ", vnp_TmnCode = " + vnp_TmnCode + ", vnp_Amount = " + vnp_Amount +
                   ", vnp_BankCode = " + vnp_BankCode + ", vnp_BankTranNo = " + vnp_BankTranNo + ", vnp_CardType = " +
                   vnp_CardType + ", vnp_PayDate = " + vnp_PayDate + ", vnp_OrderInfo = " + vnp_OrderInfo +
                   ", vnp_TransactionNo = " + vnp_TransactionNo + ", vnp_ResponseCode = " + vnp_ResponseCode +
                   ", vnp_TransactionStatus = " + vnp_TransactionStatus + ", vnp_TxnRef = " + vnp_TxnRef +
                   ", vnp_SecureHashType = " + vnp_SecureHashType + ", vnp_SecureHash = " + vnp_SecureHash;
        }
    }

    public class IPNPaymentVNPayResponseDto
    {
        /*
        * 00: Confirm Success
        * 01: Order not found
        * 02: Order already confirmed
        * 04: Invalid amount
        * 97: Invalid signature
        * 99: Unknown error
        */
        [JsonPropertyName("RspCode")] public string RspCode { get; set; }
        [JsonPropertyName("Body")] public string Message { get; set; }
    }
}