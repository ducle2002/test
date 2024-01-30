
using Abp.AutoMapper;
using Yootek.EntityDb;

namespace Yootek.Services
{
    [AutoMap(typeof(QATopic))]
    public class QATopicDto: QATopic 
    {
    }
}
