namespace Yootek.Friendships
{
    public enum FriendshipState
    {
        None = 0,
        Accepted = 1,
        Blocked = 2,
        Requesting = 3,
        IsChat = 4,
        IsDeleted = 5,
        Stranger = 6,
    }

    public enum FollowState
    {
        None = 0,
        Pending = 1,
        Following = 2,
        UnFollow = 3,
    }
}