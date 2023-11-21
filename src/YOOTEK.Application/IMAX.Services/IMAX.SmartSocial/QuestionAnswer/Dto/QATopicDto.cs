
using Abp.AutoMapper;
using IMAX.EntityDb;

namespace IMAX.Services
{
    [AutoMap(typeof(QATopic))]
    public class QATopicDto: QATopic 
    {
    }
}
