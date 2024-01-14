using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Dto.ThirdAccount
{

    public class ThirdAccoutDto
    {
        public AccountDto rocketChat { get; set; }
    }
    public class AccountDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
    }
}
