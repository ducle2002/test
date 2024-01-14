using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Chat
{
    public enum TypeMessageEnum
    {
        Link = 0,
        Text = 1,
        Image = 2,
        Video = 3,
        File = 4,
        Images = 5,
        Videos = 6,
        Files = 7,
    }
    public enum EmotionChatMessage
    {
        Like = 1
    }

    public enum ChatSide
    {
        Sender = 1,

        Receiver = 2
    }
}
