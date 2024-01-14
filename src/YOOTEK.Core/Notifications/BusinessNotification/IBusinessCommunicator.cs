using Abp.RealTime;
using Yootek.EntityDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Notifications
{
    public interface ITenantBookingCommunicator
    {
        void SendOrderToPartnerClient(IReadOnlyList<IOnlineClient> clients, Order order);
        void SendMessageToUserClient(IReadOnlyList<IOnlineClient> clients, string messager);

        Task SendNotifyTenantBusinessBookingToAdmin(Booking booking);
        Task SendNotifyTenantBusinessBookingToUser(IReadOnlyList<IOnlineClient> clients, int state);
    }
}
