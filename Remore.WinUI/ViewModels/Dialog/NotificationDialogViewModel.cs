using CommunityToolkit.Mvvm.ComponentModel;
using Remore.WinUI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Remore.WinUI.ViewModels.Dialog
{
    public partial class NotificationDialogViewModel : BaseDialogViewModel
    {
        [ObservableProperty]
        private object content;
        public NotificationDialogViewModel(string title, object content, string primaryButtonText = null)
        {
            Title = title;
            Content = content;
            CloseButtonText = "Main_ConnectToServer_CloseButton".GetLocalized();
            PrimaryButtonText = primaryButtonText;
        }
    }
}
