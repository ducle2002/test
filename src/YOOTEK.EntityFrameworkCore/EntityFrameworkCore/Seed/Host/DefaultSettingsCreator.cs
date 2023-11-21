using System.Linq;
using Microsoft.EntityFrameworkCore;
using Abp.Configuration;
using Abp.Localization;
using Abp.MultiTenancy;
using Abp.Net.Mail;
using Abp.Runtime.Security;

namespace IMAX.EntityFrameworkCore.Seed.Host
{
    public class DefaultSettingsCreator
    {
        private readonly IMAXDbContext _context;

        public DefaultSettingsCreator(IMAXDbContext context)
        {
            _context = context;
        }

        public void Create()
        {
            int? tenantId = null;

            if (IMAXConsts.MultiTenancyEnabled == false)
            {
                tenantId = MultiTenancyConsts.DefaultTenantId;
            }

            // Emailing
            AddSettingIfNotExists(EmailSettingNames.DefaultFromAddress, "admin@mydomain.com", tenantId);
            AddSettingIfNotExists(EmailSettingNames.DefaultFromDisplayName, "mydomain.com mailer", tenantId);

            // Languages
            AddSettingIfNotExists(LocalizationSettingNames.DefaultLanguage, "en", tenantId);



            AddSettingIfNotExists(EmailSettingNames.DefaultFromAddress, "nguyenminhhieubk61@gmail.com");
            AddSettingIfNotExists(EmailSettingNames.DefaultFromDisplayName, "Imax");
            AddSettingIfNotExists(EmailSettingNames.Smtp.Host, "smtp.gmail.com");
            AddSettingIfNotExists(EmailSettingNames.Smtp.Port, "587");
            AddSettingIfNotExists(EmailSettingNames.Smtp.UserName, "nguyenminhhieubk61@gmail.com");
            AddSettingIfNotExists(EmailSettingNames.Smtp.Password, SimpleStringCipher.Instance.Encrypt("@9bambutogmail"));
            AddSettingIfNotExists(EmailSettingNames.Smtp.Domain, null);
            AddSettingIfNotExists(EmailSettingNames.Smtp.EnableSsl, "true");
            AddSettingIfNotExists(EmailSettingNames.Smtp.UseDefaultCredentials, "false");
        }

        private void AddSettingIfNotExists(string name, string value, int? tenantId = null)
        {
            if (_context.Settings.IgnoreQueryFilters().Any(s => s.Name == name && s.TenantId == tenantId && s.UserId == null))
            {
                return;
            }

            _context.Settings.Add(new Setting(tenantId, null, name, value));
            _context.SaveChanges();
        }
    }
}
