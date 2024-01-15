using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Common.Enum.PostEnums
{
    public enum POST_TYPE_ENUM
    {
        DEFAULT,
        USER,
        ADMIN
    }
    
    public enum EStatusPost
    {
        Normal = 1,
        BlockComment = 2,
        BlockShare = 3,
        BlockCommentAndShare = 4,
    }
}
