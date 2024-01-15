using Abp.RealTime;
using Yootek.EntityDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Notifications.UserBillNotification
{
    public interface IUserBillRealtimeNotifier
    {
        Task NotifyUpdateStateBill(IReadOnlyList<IOnlineClient> clients, UserBill item);
    }

}
