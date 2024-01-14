using Abp.AutoMapper;
using Yootek.EntityDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Yootek.Services.Dto
{
    [AutoMap(typeof(FcmTokens))]
    public class RegisterToTenantDto
    {
        public string Token { get; set; }
        public int? TenantId { get; set; }
        public int? AppType { get; set; } = 0;
    }

    public class RegisterToApartmentDto
    {
        public List<string> Tokens { get; set; }
        public int? TenantId { get; set; }
        public string ApartmentCode { get; set; }
    }

    public class PushNotificationDto
    {
        [CanBeNull] public string GroupName { get; set; }
        public List<string> Tokens { get; set; }
        [CanBeNull] public string Title { get; set; }
        [CanBeNull] public string Subtitle { get; set; }
        [CanBeNull] public string Body { get; set; }
        [CanBeNull] public string Icon { get; set; }
        [CanBeNull] public string ClickAction { get; set; }
        [CanBeNull] public string Data { get; set; }
    }

    public class LogoutFcmDto
    {
        public string Token { get; set; }
    }
}