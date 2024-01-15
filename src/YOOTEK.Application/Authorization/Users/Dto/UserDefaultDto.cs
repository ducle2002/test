using Abp.AutoMapper;
using Yootek.EntityDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Users.Dto
{
    [AutoMapTo(typeof(Reminder))]
    public class ReminderDto : Reminder
    {
    }
}
