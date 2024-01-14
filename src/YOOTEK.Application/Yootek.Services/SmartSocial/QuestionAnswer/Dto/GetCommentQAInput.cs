using Yootek.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Services
{
    public class GetCommentQAInput: CommonInputDto
    {
        public long ForumId {  get; set; }
    }
}
