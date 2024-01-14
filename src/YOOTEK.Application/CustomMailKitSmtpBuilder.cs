using Abp.MailKit;
using Abp.Net.Mail.Smtp;
using MailKit.Net.Smtp;
using MailKit.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Yootek.MailBauilder
{
    //This is to be compatible with Office 365 Mails
    public class CustomMailKitSmtpBuilder : DefaultMailKitSmtpBuilder
    {
        private ISmtpEmailSenderConfiguration _smtpEmailSenderConfiguration;
        private IAbpMailKitConfiguration _abpMailKitConfiguration;
        public CustomMailKitSmtpBuilder(
            ISmtpEmailSenderConfiguration smtpEmailSenderConfiguration,
            IAbpMailKitConfiguration abpMailKitConfiguration
            )
            : base(smtpEmailSenderConfiguration, abpMailKitConfiguration)
        {
            this._smtpEmailSenderConfiguration = smtpEmailSenderConfiguration;
            this._abpMailKitConfiguration = abpMailKitConfiguration;
        }

        protected override void ConfigureClient(SmtpClient client)
        {
            client.Connect(
               _smtpEmailSenderConfiguration.Host,
               _smtpEmailSenderConfiguration.Port,
               SecureSocketOptions.StartTlsWhenAvailable
           );

            if (_smtpEmailSenderConfiguration.UseDefaultCredentials)
            {
                return;
            }

            client.Authenticate(
                _smtpEmailSenderConfiguration.UserName,
                _smtpEmailSenderConfiguration.Password
            );
        }

    }
}