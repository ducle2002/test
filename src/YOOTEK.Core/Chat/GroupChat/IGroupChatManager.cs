using Abp;
using Abp.Domain.Services;
using Yootek.Chat;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Yootek.GroupChats
{
    public interface IGroupChatManager : IDomainService
    {

        Task SendGroupChatMessage(UserIdentifier sender, long groupId, string message, string senderImageUrl, int typeMessage = 0);
        Task<List<string>> GetAllGroupCode(UserIdentifier user);
        Task<bool> AddMembershipGroupChat(UserIdentifier member, long groupId, GroupChatRole role = GroupChatRole.Member);
        UserGroupChat GetMemberGroupChatOrNull(UserIdentifier member, long groupId);
        Task<long> CreatGroupChat(GroupChat group);
        GroupChat GetGroupChat(long groupId);
        Task LeaveGroupChat(UserIdentifier user, string groupCode);

        Task RecallMessageGroup(UserIdentifier user, long groupId, Guid deviceMessageId, long messageId);
    }
}
