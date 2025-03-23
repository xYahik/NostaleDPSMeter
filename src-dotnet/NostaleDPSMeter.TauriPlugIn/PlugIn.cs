namespace NostaleDPSMeter.TauriPlugIn;

using Microsoft.Extensions.DependencyInjection;
using TauriDotNetBridge.Contracts;

public class PlugIn : IPlugIn
{
    public void Initialize(IServiceCollection services)
    {
        services.AddSingleton<BaseController>();
        services.AddSingleton<IHostedService, PacketSniffer>();
    }
}
