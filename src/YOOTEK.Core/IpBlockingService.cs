using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace Yootek
{
    public interface IIpBlockingService
    {
        bool IsBlocked(IPAddress ipAddress);
    }


    public class IpBlockingService : IIpBlockingService
    {
        private readonly List<string> _blockedIps;

        public IpBlockingService(IConfiguration configuration)
        {
            var blockedIps = configuration.GetValue<string>("BlockedIPs");
            _blockedIps = blockedIps.Split(',').ToList();
        }

        public bool IsBlocked(IPAddress ipAddress) => _blockedIps.Contains(ipAddress.ToString());
    }
}