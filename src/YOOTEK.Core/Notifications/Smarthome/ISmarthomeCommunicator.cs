using Abp.RealTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Notifications
{
    public interface ISmarthomeCommunicator
    {
        void NotifyUpdateSmarthomeToClient(IReadOnlyList<IOnlineClient> clients, string smarthomecode);
        void NotifyAddMemberToClient(IReadOnlyList<IOnlineClient> clients, string smarthomecode);
    }
}
