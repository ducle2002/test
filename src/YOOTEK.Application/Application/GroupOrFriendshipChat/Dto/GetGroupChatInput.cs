using IMAX.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMAX.Friendships.Dto
{
    public class GetGroupChatInput
    {
    }

    public class GetMessageGroupChatInput : CommonInputDto
    {
        public long GroupChatId { get; set; }
    }
}
