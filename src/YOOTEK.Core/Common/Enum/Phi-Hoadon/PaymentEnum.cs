namespace IMAX.Common.Enum
{
    public enum UserBillPaymentMethod
    {
        Direct = 1,
        Momo = 2,
        VNPay = 3,
        ZaloPay = 4,
        AdminVerify = 5,
        Banking = 6,

    }

    public enum UserBillPaymentStatus
    {
        Pending = 1,
        Success = 2,
        Fail = 3,
        Cancel = 4
    }

}