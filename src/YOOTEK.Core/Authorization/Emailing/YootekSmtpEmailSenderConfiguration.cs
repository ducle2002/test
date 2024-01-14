using Abp;
using Abp.Collections.Extensions;
using Abp.Configuration;
using Abp.Configuration.Startup;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Net.Mail;
using Abp.Net.Mail.Smtp;
using Abp.Runtime.Caching;
using Abp.Runtime.Security;
using Abp.Runtime.Session;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Yootek.Emailing
{
    public class YootekSmtpEmailSenderConfiguration : SmtpEmailSenderConfiguration
    {
        private readonly ITypedCache<string, Dictionary<string, SettingInfo>> _applicationSettingCache;
        private readonly ISettingDefinitionManager _settingDefinitionManager;
        protected ISettingEncryptionService SettingEncryptionService { get; }
        public ISettingStore SettingStore { get; set; }

        public YootekSmtpEmailSenderConfiguration(
            ISettingManager settingManager,
            ICacheManager cacheManager,
            ISettingDefinitionManager settingDefinitionManager,
            ISettingEncryptionService settingEncryptionService
            ) : base(settingManager)
        {
            _applicationSettingCache = cacheManager.GetApplicationSettingsCache();
            SettingStore = DefaultConfigSettingStore.Instance;
            _settingDefinitionManager = settingDefinitionManager;
            SettingEncryptionService = settingEncryptionService;
        }

        public override string Password => SimpleStringCipher.Instance.Decrypt(GetForTenantOrApplicationSettingValue(EmailSettingNames.Smtp.Password));
        /// <summary>
        /// SMTP Host name/IP.
        /// </summary>
        public override string Host
        {
            get { return GetForTenantOrApplicationSettingValue(EmailSettingNames.Smtp.Host); }
        }

        /// <summary>
        /// SMTP Port.
        /// </summary>
        public override int Port
        {
            get { return SettingManager.GetSettingValue<int>(EmailSettingNames.Smtp.Port); }
        }

        /// <summary>
        /// User name to login to SMTP server.
        /// </summary>
        public override string UserName
        {
            get { return GetForTenantOrApplicationSettingValue(EmailSettingNames.Smtp.UserName); }
        }

        /// <summary>
        /// Domain name to login to SMTP server.
        /// </summary>
        public override string Domain
        {
            get { return GetForTenantOrApplicationSettingValue(EmailSettingNames.Smtp.Domain); }
        }

        public override string DefaultFromAddress
        {
            get { return GetForTenantOrApplicationSettingValue(EmailSettingNames.DefaultFromAddress); }
        }

        public override string DefaultFromDisplayName
        {
            get { return GetForTenantOrApplicationSettingValue(EmailSettingNames.DefaultFromDisplayName); }
        }

        protected string GetForTenantOrApplicationSettingValue(string name)
        {
            var value = SettingManager.GetSettingValue(name);

            if (value.IsNullOrEmpty() || value == "127.0.0.1" || value == "25")
            {
                var settingValue = GetSettingValueForApplicationOrNullAsync(name);
                if (settingValue != null)
                {
                    value = settingValue.Value;
                }

            }

            return value;
        }

        private SettingInfo GetSettingValueForApplicationOrNullAsync(string name)
        {
            return GetApplicationSettings().GetOrDefault(name);
        }

        private Dictionary<string, SettingInfo> GetApplicationSettings()
        {
            return _applicationSettingCache.Get("ApplicationSettings", () =>
            {
                var settingValues = SettingStore.GetAllList(null, null);
                return ConvertSettingInfosToDictionary(settingValues);
            });
        }

        private Dictionary<string, SettingInfo> ConvertSettingInfosToDictionary(List<SettingInfo> settingValues)
        {
            var dictionary = new Dictionary<string, SettingInfo>();
            var allSettingDefinitions = _settingDefinitionManager.GetAllSettingDefinitions();

            foreach (var setting in allSettingDefinitions.Join(settingValues,
                definition => definition.Name,
                value => value.Name,
                (definition, value) => new
                {
                    SettingDefinition = definition,
                    SettingValue = value
                }))
            {
                if (setting.SettingDefinition.IsEncrypted)
                {
                    setting.SettingValue.Value =
                        SettingEncryptionService.Decrypt(setting.SettingDefinition, setting.SettingValue.Value);
                }

                dictionary[setting.SettingValue.Name] = setting.SettingValue;
            }

            return dictionary;
        }

    }
}