using Avalonia;
using Avalonia.Controls;
using FluentAvalonia.Core;
using System;
using System.Threading.Tasks;
using TTalk.Client.ViewModels;

namespace TTalk.Client.Views
{
    public partial class MainWindow : Window
    {

        public ListBox ListBox { get; }

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            this.FindControl<TextBox>("MessageContent").KeyDown += OnMessageContentKeyDown;
            ListBox = this.FindControl<ListBox>("MessagesListBox");
        }

        private void OnMessageContentKeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            if (e.Key == Avalonia.Input.Key.Enter || e.Key == Avalonia.Input.Key.Return)
            {
                (DataContext as MainWindowViewModel)?.SendMessage(null);
                
            }
        }

        public async Task<object?> ShowDialogHost(object model, string view)
        {
            return await DialogHost.DialogHost.Show(model, view);
        }

    }
}
