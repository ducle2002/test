using System.Threading.Tasks;

namespace Yootek.Core.EventBus
{
    public interface IDynamicIntegrationEventHandler
    {
        Task Handle(dynamic eventData);
    }
}


