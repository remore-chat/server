using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;

namespace TTalk.WinUI.Contracts.Services
{
    public interface INavigationService
    {
        event NavigatedEventHandler Navigated;

        bool CanGoBack { get; }

        Frame Frame { get; set; }

        bool NavigateTo(string pageKey, object parameter = null, bool clearNavigation = false, NavigationTransitionInfo navigationTransitionInfo = null);
        bool NavigateTo(Type pageType, object parameter = null, bool clearNavigation = false, NavigationTransitionInfo navigationTransitionInfo = null);

        bool GoBack();
    }
}
