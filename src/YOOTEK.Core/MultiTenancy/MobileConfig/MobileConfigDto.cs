using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.MultiTenancy
{
    public class MobileConfig
    {
        public string[] SocialConfig { get; set; }
        public string[] CommunityConfig { get; set; }
        public int TypeConfig { get; set; }
        public string MobileVersion { get; set; }
    }
}
