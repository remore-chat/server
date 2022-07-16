using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace Remore.WinUI.ViewModels.Dialog
{
    public partial class BaseDialogViewModel : ObservableObject
    {
        [ObservableProperty]
        protected string closeButtonText;
        [ObservableProperty]
        protected string primaryButtonText;
        [ObservableProperty]
        protected string title;
        [ObservableProperty]
        [AlsoNotifyChangeFor(nameof(IsNotBusy))]
        protected bool isBusy;
        public bool IsNotBusy => !IsBusy;
        public SolidColorBrush Background => 
            (App.MainWindow.Content as FrameworkElement).RequestedTheme == ElementTheme.Dark ? 
            new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)) :
            // #121212
            new SolidColorBrush(Color.FromArgb(255, 18, 18, 18));
    }
}
