using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YOOTEK.Authorization.AccountErps
{
    public class RegisterErpOutput
    {
        public bool CanLogin { get; set; }
        public TimeSpan TimeCodeExpire { get; set; }
    }
}
