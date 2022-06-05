using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.VisualBasic.Devices;
using System.Collections.Generic;
using System.Linq;
using TTalk.WinUI.Models;
using TTalk.WinUI.ViewModels;

namespace TTalk.WinUI.Views
{
    [INotifyPropertyChanged]
    public sealed partial class ServerSettingsOverviewView : Page
    {
        [ObservableProperty]
        private ServerSettingsViewModel viewModel;

        public ServerSettingsOverviewView()
        {
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel = e.Parameter as ServerSettingsViewModel;

            InitializeComponent();
            base.OnNavigatedTo(e);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.Name = (sender as TextBox).Text;
        }

        private void NumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            ViewModel.MaxClients = (int)sender.Value;
        }
    }
}
