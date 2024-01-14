using Microsoft.Extensions.Configuration;

namespace Yootek.Configuration
{
    public interface IAppConfigurationAccessor
    {
        IConfigurationRoot Configuration { get; }
    }
}
