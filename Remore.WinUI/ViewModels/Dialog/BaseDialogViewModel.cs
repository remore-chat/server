using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    }
}
