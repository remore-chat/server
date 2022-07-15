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
    public sealed partial class JoinServerDialog : ContentDialog
    {
        public JoinServerDialog()
        {
            this.InitializeComponent();
            var vm = App.GetService<JoinServerDialogViewModel>(); ;
            vm.ServerPicked += OnServerPicked;
            DataContext = vm;
        }

        private void OnServerPicked(object sender, string e)
        {
            IsConnectionFromFavorites = true;
            this.Hide();
        }
        public bool IsConnectionFromFavorites { get; set; }
        public string ConnectionAddress { get => (DataContext as JoinServerDialogViewModel).Address; }
        public bool ShouldServerBeAddedInFavoritesAfterConnect { get => (DataContext as JoinServerDialogViewModel).ShouldServerBeAddedInFavoritesAfterConnect; }
    }
}
