using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.VisualBasic.Devices;
using System.Collections.Generic;
using System.Linq;
using TTalk.WinUI.Contracts.Services;
using TTalk.WinUI.Models;
using TTalk.WinUI.ViewModels;

namespace TTalk.WinUI.Views
{
    [INotifyPropertyChanged]
    public sealed partial class ServerSettingsPage : Page
    {
        private ServerSettingsViewModel referenceViewModel;

        [ObservableProperty]
        private ServerSettingsViewModel viewModel;

        [ObservableProperty]
        private bool isSettingsChanged;

        public ServerSettingsPage()
        {
            InitializeComponent();
            saveButton.Click += OnSaveButtonClick;
            resetButton.Click += OnResetButtonClick;
        }

        private void OnResetButtonClick(object sender, RoutedEventArgs e)
        {
            var navService = App.GetService<INavigationService>();
            navService.NavigateTo(typeof(MainViewModel).FullName, null, true, new SuppressNavigationTransitionInfo());
            navService.NavigateTo(typeof(ServerSettingsViewModel).FullName, referenceViewModel, true, new SuppressNavigationTransitionInfo());
        }

        private void OnSaveButtonClick(object sender, RoutedEventArgs e)
        {
            ViewModel.SaveServerInfo();
            App.GetService<INavigationService>().NavigateTo(typeof(MainViewModel).FullName, null, true);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var viewmodel = e.Parameter as ServerSettingsViewModel;
            referenceViewModel = viewmodel;
            ViewModel = viewmodel.Clone() as ServerSettingsViewModel;
            ViewModel.PropertyChanged += OnViewModelPropertyChanged;
            navigation.SelectedItem = navigation.MenuItems.First();
            base.OnNavigatedTo(e);
        }

        private void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            IsSettingsChanged = ServerSettingsViewModel.IsSettingsChanged(viewModel, referenceViewModel);
        }

        private void OnNavigationViewSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var tag = (args.SelectedItem as FrameworkElement).Tag.ToString();
            switch (tag)
            {
                case "overview":
                    content.Navigate(typeof(ServerSettingsOverviewView), ViewModel, new DrillInNavigationTransitionInfo());
                    break;
            }
        }
    }
}
