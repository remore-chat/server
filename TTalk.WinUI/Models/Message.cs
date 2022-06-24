using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTalk.WinUI.Networking;
using TTalk.WinUI.ViewModels;
using Windows.Storage.Streams;

namespace TTalk.WinUI.Models
{
    public class Message
    {

        public string Id { get; set; }
        public string ChannelId { get; set; }
        public string Username { get; set; }
        public string Text { get; set; }
        public List<Attachment> Attachments { get; set; }
        public DateTime CreatedAt { get; set; }
    }


    public partial class Attachment : ObservableObject
    {
        [ObservableProperty]
        private string id;
        [ObservableProperty]
        private string fileId;
        [ObservableProperty]
        private string contentType;
        [ObservableProperty]
        private int loadingProgress;
        public bool IsImage => ContentType == "image/jpeg" || ContentType == "image/png";
        [ObservableProperty]
        [AlsoNotifyChangeFor(nameof(IsNotLoaded))]
        private bool isLoaded;
        public bool IsNotLoaded => !IsLoaded;
        private ImageSource imageSource;
        private ContentDialog _dialog;

        public ImageSource ImageSource
        {
            get
            {
                if (imageSource != null)
                {
                    IsLoaded = true;
                    return imageSource;
                }
                else
                {
                    var image = new BitmapImage();
                    imageSource = image;
                    _ = Task.Run(async () => await DownloadImage());
                    return imageSource;
                }
            }
        }

        private async Task DownloadImage()
        {
            var addr = App.GetService<MainViewModel>().ServerAddress;
            var file = await new TTalkFileClient(addr.Ip, addr.Port, (progress) => App.MainWindow.DispatcherQueue.TryEnqueue(() => LoadingProgress = progress)).DownloadFile(FileId);
            App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
            {
                if (file.Error == null)
                {
                    var bitmapImage = new BitmapImage();

                    using (var stream = new InMemoryRandomAccessStream())
                    {
                        using (var writer = new DataWriter(stream))
                        {
                            writer.WriteBytes(file.Data);
                            await writer.StoreAsync();
                            await writer.FlushAsync();
                            writer.DetachStream();
                        }

                        stream.Seek(0);
                        await bitmapImage.SetSourceAsync(stream);
                    }
                    imageSource = bitmapImage;

                    IsLoaded = true;
                    OnPropertyChanged(nameof(ImageSource));
                }
            });
        }
        //TODO: Move it somewhere else, it doesn't fit there
        internal void Maximize()
        {
            App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
            {
                var grid = new Grid();
                grid.ColumnDefinitions.Add(new() { Width = Microsoft.UI.Xaml.GridLength.Auto });
                grid.Children.Add(new Image() { MaxWidth = 500, Source = imageSource });
                _dialog = new ContentDialog()
                {
                    Content = grid,
                    MinWidth = 1280,
                    XamlRoot = App.MainWindow.Content.XamlRoot,
                    CloseButtonText = "Close"
                };
                await _dialog.ShowAsync(ContentDialogPlacement.Popup);
            });
        }
    }
}
