using System;

namespace Yootek.Yootek.Services.Yootek.Payments.Dto;

public class OnepayVerificationDto
{
    public DateTime CreatedAt { get; set; }
    public int Id { get; set; }
    public int PaymentId { get; set; }
    public string BankId { get; set; }
    public string MerchantId { get; set; }
    public int TransactionId { get; set; }
    public string OrderInfo { get; set; }
    public int Amount { get; set; }
    public string MerchantTransactionId { get; set; }
    public DateTime DateTime { get; set; }
    public int ResponseCode { get; set; }
    public string Status { get; set; }
    public int PayType { get; set; }
}