using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.VisualBasic.Devices;
using System.Collections.Generic;
using System.Linq;
using Remore.WinUI.Models;
using Remore.WinUI.ViewModels;
using System;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml.Media;

namespace Remore.WinUI.Views
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel ViewModel { get; }
        public MainPage()
        {
            ViewModel = App.GetService<MainViewModel>();
            InitializeComponent();
            MessageContent.KeyDown += OnMessageContentKeyDown;
            ViewModel.MessagesListBox = MessagesListBox;
        }

        private void MessagesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void OnMessageContentKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            //
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

        private void NewLine(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
        {
            var txtB = MessageContent;
            if (txtB == null)
                return;
            var caretIdx = txtB.SelectionStart;

            if (string.IsNullOrEmpty(MessageContent.Text))
                MessageContent.Text += Environment.NewLine;
            else
                MessageContent.Text = MessageContent.Text.Insert(caretIdx, "\r");

            txtB.SelectionStart = caretIdx + 1;
        }

        private void Send(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
        {
            ViewModel.SendMessage(MessageContent.Text);
            MessageContent.Text = "";
        }

        private void MarkdownTextBlock_LinkClicked(object sender, CommunityToolkit.WinUI.UI.Controls.LinkClickedEventArgs e)
        {
            DispatcherQueue.TryEnqueue(async () =>
            {
                var res = await new ContentDialog()
                {
                    Title = "Hang on",
                    Content = new MarkdownTextBlock()
                    {
                        Text = $"This link will take you to **{e.Link}**. Are you sure you want to go there?",
                    },
                    XamlRoot = this.Content.XamlRoot,
                    CloseButtonText = "Cancel",
                    PrimaryButtonText = "Yes, take me there",
                }.ShowAsync(ContentDialogPlacement.InPlace);
                if (res == ContentDialogResult.Primary)
                    LinkHandler.OpenBrowser(e.Link);
            });
        }
    }
}
