using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;
using TTalk.Client.Views;

namespace TTalk.Client.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
        public MainWindow MainWindow => (App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).MainWindow as MainWindow;
    }
}
