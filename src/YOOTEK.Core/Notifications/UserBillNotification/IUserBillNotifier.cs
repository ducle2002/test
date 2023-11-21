using Abp.RealTime;
using IMAX.EntityDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMAX.Notifications.UserBillNotification
{
    public interface IUserBillRealtimeNotifier
    {
        Task NotifyUpdateStateBill(IReadOnlyList<IOnlineClient> clients, UserBill item);
    }

}
