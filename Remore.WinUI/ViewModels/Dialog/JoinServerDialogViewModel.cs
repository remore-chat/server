using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.UI.Controls.TextToolbarSymbols;
using Microsoft.UI.Dispatching;
using Remore.Client.Core;
using Remore.WinUI.Contracts.Services;
using Remore.WinUI.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Remore.WinUI.ViewModels.Dialog
{
    public partial class JoinServerDialogViewModel : BaseDialogViewModel
    {
        private ILocalSettingsService _settingsService;
        private DispatcherQueue _dispatcher;
        private string _username;
        public JoinServerDialogViewModel(ILocalSettingsService settingsService)
        {
            IsBusy = true;
            _settingsService = settingsService;
            _dispatcher = App.MainWindow.DispatcherQueue;
            CloseButtonText = "Main_ConnectToServer_CloseButton".GetLocalized();
            Title = "Main_ConnectToServer_Title".GetLocalized();
            ConnectCommand = new RelayCommand<string>((address) =>
            {
                ServerPickedCallback(address);
            });
            RemoveServerFromFavoritesCommand = new RelayCommand<string>(async (address) =>
            {
                App.MainWindow.DispatcherQueue.TryEnqueue(() =>
                {
                    var item = ServerList.FirstOrDefault(x => x.Address == address);
                    ServerList.Remove(item);
                });
                var list = await _settingsService.ReadSettingAsync<List<string>>(SettingsViewModel.FavoritesSettingsKey);
                list.Remove(address);
                await settingsService.SaveSettingAsync(SettingsViewModel.FavoritesSettingsKey, list);
            });
            _ = Initialize();
        }

        [ObservableProperty]
        private bool isNicknameMisconfigured;
        [ObservableProperty]
        private string misconfiguredNicknameMessage;
        [ObservableProperty]
        private ObservableCollection<FavoriteServer> serverList;
        private int selectedTab;
        public int SelectedTab
        {
            get => selectedTab;
            set
            {
                SetProperty(ref selectedTab, value);
                SelectedTabChanged(value);
            }
        }

        public RelayCommand<string> ConnectCommand { get; set; }
        public RelayCommand<string> RemoveServerFromFavoritesCommand { get; }

        public event EventHandler<string> ServerPicked;

        [ObservableProperty]
        private string address;
        [ObservableProperty]
        private bool shouldServerBeAddedInFavoritesAfterConnect;
        private async Task Initialize()
        {
            _username = await _settingsService.ReadSettingAsync<string>(SettingsViewModel.UsernameSettingsKey);
            if (string.IsNullOrWhiteSpace(_username) || _username.Length < 3)
            {
                _dispatcher.TryEnqueue(() =>
                {
                    MisconfiguredNicknameMessage = "Main_ConnectToServer_InvalidNicknameContent".GetLocalized();
                    Title = "Main_ConnectToServer_InvalidNicknameTitle".GetLocalized();
                    IsBusy = false;
                    IsNicknameMisconfigured = true;
                });
                return;
            }
            PrimaryButtonText = "Main_ConnectToServer_ConnectButton".GetLocalized();
            ServerList = new((await _settingsService.ReadSettingAsync<List<string>>(SettingsViewModel.FavoritesSettingsKey)).Select(x => new FavoriteServer(x)));

            IsBusy = false;
            IsNicknameMisconfigured = false;
        }

        private void ServerPickedCallback(string address)
        {
            ShouldServerBeAddedInFavoritesAfterConnect = false;
            Address = address;
            ServerPicked?.Invoke(this, address);
        }

        private void SelectedTabChanged(int tab)
        {
            //Remove connect button if Favorites tab is selected
            PrimaryButtonText = tab == 0 ? "Main_ConnectToServer_ConnectButton".GetLocalized() : null;
        }
    }

    public partial class FavoriteServer : ObservableObject
    {
        public FavoriteServer(string address)
        {
            this.address = address;
            _dispatcher = App.MainWindow.DispatcherQueue;
            _ = StartConnection();
        }

        [ObservableProperty]
        private string connectionStatusMessage;
        [ObservableProperty]
        [AlsoNotifyChangeFor(nameof(IsNotConnecting))]
        private bool isConnecting;
        [ObservableProperty]
        private bool isConnectionSucceeded;
        public bool IsNotConnecting => !IsConnecting;
        [ObservableProperty]
        private string address;

        private readonly DispatcherQueue _dispatcher;
        private async Task StartConnection()
        {
            _dispatcher.TryEnqueue(() =>
            {
                IsConnecting = true;
                IsConnectionSucceeded = false;
                ConnectionStatusMessage = string.Format("Main_ConnectToServerFavorites_ConnectingToServer".GetLocalized(), address);
            });
#if DEBUG
            await Task.Delay(3000);
#endif
            var ip = address.Split(":")[0];
            int port = 0;
            if (!IPAddress.TryParse(ip, out var _))
            {
                var ep = NetworkUtility.GetEndpointForHostname(address);
                ip = ep.Address;
                port = ep.Port;
            }
            else
            {
                port = Convert.ToInt32(address.Split(":")[1]);
            }

            var queryClient = new RemoreQueryClient(ip, port);
            var serverInfo = await queryClient.GetServerInfo();
            if (serverInfo == null)
            {
                _dispatcher.TryEnqueue(() =>
                {
                    IsConnecting = false;
                    IsConnectionSucceeded = false;
                    ConnectionStatusMessage = string.Format("Main_ConnectToServerFavorites_FailedToConnect".GetLocalized(), address);
                });
            }
            else
            {
                _dispatcher.TryEnqueue(() =>
                {
                    IsConnecting = false;
                    if (serverInfo.ServerVersion != SettingsViewModel.ClientVersion)
                    {
                        IsConnectionSucceeded = false;
                        ConnectionStatusMessage = string.Format("Main_ConnectToServer_VersionDontMatch".GetLocalized(), serverInfo.ServerName, SettingsViewModel.ClientVersion, serverInfo.ServerVersion);
                    }
                    else
                    {
                        IsConnectionSucceeded = true;
                        ConnectionStatusMessage = $"{serverInfo.ServerName} - {serverInfo.ServerVersion} ({serverInfo.ClientsConnected}/{serverInfo.MaxClients})";
                    }
                });
            }
        }
    }
}
