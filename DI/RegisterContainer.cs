using Microsoft.Extensions.DependencyInjection;
using MSTeamsPublishing.Services;
using Sitecore.DependencyInjection;

namespace MSTeamsPublishing.DI
{
    public class RegisterContainer : IServicesConfigurator
    {
        public void Configure(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IMsTeamsConnectorService, MsTeamsConnectorService>();
        }
    }
}