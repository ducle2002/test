using IMAX.EntityDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMAX.Services.Dto
{
    public class ExportForumDto : ForumDto
    {
        public List<CommentForumDto> Comments { get; set; }
    }
   
}
