using Microsoft.Extensions.DependencyInjection;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Cmd.Setup;

internal static class ApplicationSetup
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        return services;
    }
}