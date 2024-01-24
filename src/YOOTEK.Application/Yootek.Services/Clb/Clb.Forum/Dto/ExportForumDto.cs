using Yootek.EntityDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Services.Dto
{
    public class ExportForumPostDto : ForumPostDto
    {
        public List<CommentForumDto> Comments { get; set; }
    }
   
}
