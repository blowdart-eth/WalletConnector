using Blazored.LocalStorage;
using Microsoft.Extensions.DependencyInjection;
using WalletConnector.Core;

namespace WalletConnector.Metamask
{
    public static class MetamaskExtensions
    {
        public static IServiceCollection AddMetamaskIntegration(this IServiceCollection services)
        {
            services.AddBlazoredLocalStorage();
            services.AddSingleton<IMetamaskInterop, MetamaskBlazorInterop>();
            services.AddSingleton<MetamaskInterceptor>();
            services.AddSingleton<MetamaskHostProvider>();
            services.AddSingleton<IEthereumHostProvider>(r => r.GetRequiredService<MetamaskHostProvider>());

            return services;
        }
    }
}
