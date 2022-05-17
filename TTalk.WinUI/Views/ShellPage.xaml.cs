using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

using TTalk.WinUI.Contracts.Services;
using TTalk.WinUI.ViewModels;

using Windows.System;

namespace TTalk.WinUI.Views
{
    // TODO: Change the icons and titles for all NavigationViewItems in ShellPage.xaml.
    public sealed partial class ShellPage : Page
    {
        private readonly KeyboardAccelerator _altLeftKeyboardAccelerator = BuildKeyboardAccelerator(VirtualKey.Left, VirtualKeyModifiers.Menu);
        private readonly KeyboardAccelerator _backKeyboardAccelerator = BuildKeyboardAccelerator(VirtualKey.GoBack);

        public ShellViewModel ViewModel { get; }

        //MainViewModel should always be the same
        public MainViewModel MainViewModel { get; }

        public ShellPage(ShellViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
            ViewModel.NavigationService.Frame = shellFrame;
            ViewModel.NavigationViewService.Initialize(navigationView);
            navigationView.ItemInvoked += OnNavigationViewItemInvoked;
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
