using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.VisualBasic.Devices;
using System.Collections.Generic;
using System.Linq;
using TTalk.WinUI.Models;
using TTalk.WinUI.ViewModels;

namespace TTalk.WinUI.Views
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel ViewModel { get; }
        public MainPage()
        {
            ViewModel = App.GetService<MainViewModel>();
            InitializeComponent();
            MessageContent.KeyDown += OnMessageContentKeyDown;
        }

        private void MessagesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void OnMessageContentKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter && new Keyboard().ShiftKeyDown)
            {
                MessageContent.TextWrapping = TextWrapping.Wrap;
                MessageContent.AcceptsReturn = true;
                MessageContent.Text += "\n";
                MessageContent.SelectionStart = MessageContent.Text.Length;
                MessageContent.SelectionLength = 0;
                MessageContent.AcceptsReturn = false;
                return;
            }
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                ViewModel.SendMessage(MessageContent.Text);
                MessageContent.Text = "";
            }
        }


        private void StackPanel_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == Microsoft.UI.Input.PointerDeviceType.Mouse)
            {
                var properties = e.GetCurrentPoint(this).Properties;
                
                if (properties.IsRightButtonPressed)
                {
                    return;
                }
            }
            var channel = (e.OriginalSource as FrameworkElement).DataContext as Channel;
            if (channel == null)
                return;
            DispatcherQueue.TryEnqueue(() =>
            {
                if (channel.ChannelType == Library.Enums.ChannelType.Voice)
                {
                    if (channel.Id == ViewModel.CurrentChannel?.Id)
                        return;
                    if (ViewModel.CurrentChannel != null)
                        ViewModel.CurrentChannel.IsSelected = false;
                    channel.JoinChannel.Execute(null);
                }
                else
                {
                    if (channel.Id == ViewModel.CurrentTextChannel?.Id)
                        return;
                    if (ViewModel.CurrentTextChannel != null)
                        ViewModel.CurrentTextChannel.IsSelected = false;
                    channel.JoinChannel.Execute(null);
                }
            });
        }

        private void Border_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            var border = sender as Border;
            var attachment = border.Tag as Attachment;
            attachment.Maximize();
        }
    }
}
