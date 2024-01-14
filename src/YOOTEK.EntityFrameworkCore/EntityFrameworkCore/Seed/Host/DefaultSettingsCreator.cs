using System.Linq;
using Microsoft.EntityFrameworkCore;
using Abp.Configuration;
using Abp.Localization;
using Abp.MultiTenancy;
using Abp.Net.Mail;
using Abp.Runtime.Security;

namespace Yootek.EntityFrameworkCore.Seed.Host
{
    public class DefaultSettingsCreator
    {
        private readonly YootekDbContext _context;

        public DefaultSettingsCreator(YootekDbContext context)
        {
            _context = context;
        }

        public void Create()
        {
            int? tenantId = null;

            if (YootekConsts.MultiTenancyEnabled == false)
            {
                tenantId = MultiTenancyConsts.DefaultTenantId;
            }
            // Languages
            AddSettingIfNotExists(LocalizationSettingNames.DefaultLanguage, "en", tenantId);



            AddSettingIfNotExists(EmailSettingNames.DefaultFromAddress, "nguyenminhhieubk61@gmail.com");
            AddSettingIfNotExists(EmailSettingNames.DefaultFromDisplayName, "Yoolife");
            AddSettingIfNotExists(EmailSettingNames.Smtp.Host, "smtp.gmail.com");
            AddSettingIfNotExists(EmailSettingNames.Smtp.Port, "587");
            AddSettingIfNotExists(EmailSettingNames.Smtp.UserName, "nguyenminhhieubk61@gmail.com");
            AddSettingIfNotExists(EmailSettingNames.Smtp.Password, SimpleStringCipher.Instance.Encrypt("hieudz"));
            AddSettingIfNotExists(EmailSettingNames.Smtp.Domain, "Yoolife");
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
