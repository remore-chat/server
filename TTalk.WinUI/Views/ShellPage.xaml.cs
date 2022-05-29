using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

using TTalk.WinUI.Contracts.Services;
using TTalk.WinUI.Services;
using TTalk.WinUI.ViewModels;

using Windows.System;

namespace TTalk.WinUI.Views
{
    public sealed partial class ShellPage : Page
    {
        private readonly KeyboardAccelerator _altLeftKeyboardAccelerator = BuildKeyboardAccelerator(VirtualKey.Left, VirtualKeyModifiers.Menu);
        private readonly KeyboardAccelerator _backKeyboardAccelerator = BuildKeyboardAccelerator(VirtualKey.GoBack);

        public ShellViewModel ViewModel { get; }
        public ILocalSettingsService Settings { get; }
        public LocalizationService Localization { get; }

        public ShellPage(ShellViewModel viewModel, ILocalSettingsService settings, LocalizationService localization)
        {
            ViewModel = viewModel;
            Settings = settings;
            Localization = localization;
            InitializeComponent();
            ViewModel.NavigationService.Frame = shellFrame;
            ViewModel.NavigationViewService.Initialize(navigationView);
            navigationView.ItemInvoked += OnNavigationViewItemInvoked;
            Localization.LanguageUpdated += OnLanguageUpdated;
        }

        private void OnLanguageUpdated(object sender, object e)
        {
            foreach (var item in navigationView.MenuItems)
            {
                var nvi = (item as NavigationViewItem);
                nvi.Content = App.GetService<LocalizationService>().GetString(nvi.Tag.ToString());
            }
        }

        private void OnNavigationViewItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
           

        }

        private void OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            // Keyboard accelerators are added here to avoid showing 'Alt + left' tooltip on the page.
            // More info on tracking issue https://github.com/Microsoft/microsoft-ui-xaml/issues/8
            KeyboardAccelerators.Add(_altLeftKeyboardAccelerator);
            KeyboardAccelerators.Add(_backKeyboardAccelerator);
        }

        private static KeyboardAccelerator BuildKeyboardAccelerator(VirtualKey key, VirtualKeyModifiers? modifiers = null)
        {
            var keyboardAccelerator = new KeyboardAccelerator() { Key = key };
            if (modifiers.HasValue)
            {
                keyboardAccelerator.Modifiers = modifiers.Value;
            }

            keyboardAccelerator.Invoked += OnKeyboardAcceleratorInvoked;
            return keyboardAccelerator;
        }

        private static void OnKeyboardAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            var navigationService = App.GetService<INavigationService>();
            var result = navigationService.GoBack();
            args.Handled = result;
        }
    }
}
