using Microsoft.Extensions.Logging;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Web3;
using WalletConnector.Core;

namespace WalletConnector.Metamask
{
    public class MetamaskHostProvider : IEthereumHostProvider
    {
        private readonly MetamaskInterceptor _metamaskInterceptor;
        private readonly IMetamaskInterop _metamaskInterop;
        
        public string Name => "Metamask";

        public static MetamaskHostProvider? Current { get; private set; }
        public bool Available { get; private set; }
        public string? SelectedAccount { get; private set; }
        public Network Network { get; private set; }
        public bool Enabled { get; private set; }

        public event Func<string, Task>? SelectedAccountChanged;
        public event Func<Network, Task>? NetworkChanged;
        public event Func<bool, Task>? AvailabilityChanged;
        public event Func<bool, Task>? EnabledChanged;

        private readonly ILogger<MetamaskHostProvider>?  _logger;

        public MetamaskHostProvider(IMetamaskInterop metamaskInterop, ILogger<MetamaskHostProvider>? logger)
        {
            _metamaskInterop = metamaskInterop;
            _metamaskInterceptor = new MetamaskInterceptor(_metamaskInterop, this);
            _logger = logger;
            Current = this;
        }

        public async Task<bool> CheckProviderAvailabilityAsync()
        {
            var result = await _metamaskInterop.CheckMetamaskAvailability();
            await ChangeMetamaskAvailableAsync(result);
            return result;
        }

        public Task<Web3> GetWeb3Async()
        {
            var web3 = new Web3 {Client = {OverridingRequestInterceptor = _metamaskInterceptor}};
            return Task.FromResult(web3);
        }

        public async Task<string?> EnableProviderAsync()
        {
            var selectedAccount = await _metamaskInterop.EnableEthereumAsync();
            Enabled = !string.IsNullOrEmpty(selectedAccount);
            if (!Enabled) return null;

            SelectedAccount = selectedAccount;

            if (SelectedAccountChanged != null)
            {
                await SelectedAccountChanged.Invoke(selectedAccount);
            }

            if (EnabledChanged != null)
            {
                await EnabledChanged.Invoke(Enabled);
            }

            return selectedAccount;

        }

        public async Task<string?> GetProviderSelectedAccountAsync()
        {
            var result = await _metamaskInterop.GetSelectedAddress();
            await ChangeSelectedAccountAsync(result);
            return result;
        }

        public async Task<Network> GetProviderNetworkAsync()
        {
            var result = await _metamaskInterop.GetNetwork();
            var network = (Network) Convert.ToInt32(result , 16);
            await ChangeNetworkAsync(network);
            return network;
        }

        public async Task<string> GetProviderEncryptionPublicKey()
        {
            var account = await _metamaskInterop.GetSelectedAddress();
            return await _metamaskInterop.GetEncryptionPublicKey(account);
        }

        public async Task<string> Encrypt(string encryptionPublicKey, string message)
        {
            return await _metamaskInterop.Encrypt(encryptionPublicKey, message);
        }

        public async Task<string> Decrypt(string encryptedMessage)
        {
            var account = await _metamaskInterop.GetSelectedAddress();
            return await _metamaskInterop.Decrypt(encryptedMessage, account);
        }

        public async Task<string> SignMessageAsync(string message)
        {
            return await _metamaskInterop.SignAsync(message.ToHexUTF8());
        }

        public async Task ChangeSelectedAccountAsync(string selectedAccount)
        {
            if (SelectedAccount != selectedAccount)
            {
                SelectedAccount = selectedAccount;

                if (SelectedAccountChanged != null)
                {
                    _logger?.LogDebug("{Event}: {Value}", nameof(SelectedAccountChanged), selectedAccount);
                    await SelectedAccountChanged.Invoke(selectedAccount);
                }
            }
        }

        public async Task ChangeNetworkAsync(Network network)
        {
            if (Network != network)
            {
                Network = network;
                if (NetworkChanged != null)
                {
                    _logger?.LogDebug("{Event}: {Value}", nameof(NetworkChanged), network);
                    await NetworkChanged.Invoke(network);
                }
            }
        }

        public async Task ChangeMetamaskAvailableAsync(bool available)
        {
            Available = available;

            if (AvailabilityChanged != null)
            {
                _logger?.LogDebug("{Event}: {Value}", nameof(AvailabilityChanged), available);
                await AvailabilityChanged.Invoke(available);
            }
        }
    }
}
