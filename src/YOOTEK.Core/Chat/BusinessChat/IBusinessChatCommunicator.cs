using Abp;
using Abp.RealTime;
using IMAX.Chat.BusinessChat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMAX.Chat
{
    public interface IBusinessChatCommunicator
    {
        Task SendMessageToClient(IReadOnlyList<IOnlineClient> clients, BusinessChatMessage message, BusinessChatMessage messageReplied = null);
        Task SendAllUnreadBusinessChatMessageAsReadToClients(IReadOnlyList<IOnlineClient> clients, UserIdentifier user);
        Task SendDeleteBusinessMessageToClient(IReadOnlyList<IOnlineClient> clients, BusinessChatMessage mess);
    }
}
