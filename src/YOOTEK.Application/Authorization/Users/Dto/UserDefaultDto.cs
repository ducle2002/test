using Abp.AutoMapper;
using IMAX.EntityDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMAX.Users.Dto
{
    [AutoMapTo(typeof(Reminder))]
    public class ReminderDto : Reminder
    {
    }
}
