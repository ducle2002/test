using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Application.Configuration.Tenant.Dto
{
    public class TenantSettingsEditDto
    {
        /*[Required]
        public GeneralSettingsEditDto General { get; set; }
        [Required]
        public EmailSettingsEditDto Email { get; set; }*/
        [Required]
        public TimeScheduleCheckBillSettingsEditDto TimeScheduleCheckBill { get; set; }
        [Required]
        public UserBillSettingsEditDto UserBillSetting { get; set; }
        public BankTransferSettingDto BankTransferSetting { get; set; }
    }

    public class BankInfoDto
    {   
        public string BankInfo { get; set; }
    }
}
