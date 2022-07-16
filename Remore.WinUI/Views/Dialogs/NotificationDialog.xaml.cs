using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Remore.WinUI.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;


namespace Remore.WinUI.Views.Dialogs
{
    public sealed partial class NotificationDialog : ContentDialog
    {
        public NotificationDialog(NotificationDialogViewModel viewModel)
        {
            DataContext = viewModel;
            this.InitializeComponent();
        }
    }
}
