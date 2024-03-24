namespace Yootek.Emailing
{
    public interface IEmailTemplateProvider
    {
        string GetDefaultTemplate(int? tenantId);
        string GetUserBillTemplate(int? tenantId);
        string GetOTPTemplate();
    }
}
