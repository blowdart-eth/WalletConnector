using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using WalletConnector.Core;
using WalletConnector.Extensions;

namespace WalletConnector.Components;

public abstract class WalletLayoutComponent : LayoutComponentBase, IDisposable
{
    [Inject]
    public HttpClient Http { get; set; } = null!;

    [Inject]
    public NavigationManager Nav { get; set; } = null!;

    [Inject]
    public IJSRuntime Js { get; set; } = null!;

    [Inject]
    public IEthereumHostProvider HostProvider { get; set; } = null!;

    [Inject]
    public ILocalStorageService LocalStorage { get; set; } = null!;

    [Inject]
    public ILogger? Logger { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        HostProvider.AvailabilityChanged += AvailabilityChanged;
        HostProvider.EnabledChanged += EnabledChanged;
        HostProvider.SelectedAccountChanged += SelectedAccountChanged;
        HostProvider.NetworkChanged += NetworkChanged;
    }

    private Task EnabledChanged(bool enabled)
    {
        Logger?.LogDebug("enabled changed to {enabled}", enabled);
        StateHasChanged();
        return Task.CompletedTask;
    }

    private Task AvailabilityChanged(bool available)
    {
        Logger?.LogDebug("availability changed to {available}", available);
        StateHasChanged();
        return Task.CompletedTask;
    }

    private async Task SelectedAccountChanged(string account)
    {
        Logger?.LogDebug("selected account changed to {account}", account);
        await Http.TryAddBearerTokenAsync(account, LocalStorage);
        StateHasChanged();
    }
    
    private Task NetworkChanged(Network network)
    {
        Logger?.LogDebug("network changed to {network}", network);
        StateHasChanged();
        return Task.CompletedTask;
    }

    public virtual void Dispose()
    {
        HostProvider.AvailabilityChanged -= AvailabilityChanged;
        HostProvider.EnabledChanged -= EnabledChanged;
        HostProvider.SelectedAccountChanged -= SelectedAccountChanged;
        HostProvider.NetworkChanged -= NetworkChanged;

        GC.SuppressFinalize(this);
    }
}