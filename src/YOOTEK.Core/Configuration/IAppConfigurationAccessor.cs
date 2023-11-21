using Microsoft.Extensions.Configuration;

namespace IMAX.Configuration
{
    public interface IAppConfigurationAccessor
    {
        IConfigurationRoot Configuration { get; }
    }
}
