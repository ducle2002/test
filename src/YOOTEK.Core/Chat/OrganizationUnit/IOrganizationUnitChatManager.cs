using Abp;
using Abp.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Chat
{
    public interface IOrganizationUnitChatManager : IDomainService
    {
        Task SendMessageOrgAsync(UserIdentifier sender, UserIdentifier receiver, string message,string fileUrl, string senderTenancyName, string senderUserName, string senderImageUrl, long? MessageRepliedId, int TypeMessage = 0, bool isAdmin = false);
        Task DeleteMessageOrgAsync(UserIdentifier sender, UserIdentifier receiver, Guid deviceMessageId, long id);

        Task<ChatMessage> FindMessageAsync(int id, long userId);

    }
}
