using System.Threading.Tasks;

namespace IMAX.Core.EventBus
{
    public interface IDynamicIntegrationEventHandler
    {
        Task Handle(dynamic eventData);
    }
}


