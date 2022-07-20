using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Data;
using Remore.Client.Core;
using Remore.Library.Models;
using Windows.Foundation;

namespace Remore.WinUI.Controls
{
    public class Conversation : ObservableCollection<ChannelMessage>, ISupportIncrementalLoading
    {
        private string _channelId;
        private RemoreClient _client;

        public Conversation(string channelId, RemoreClient client)
        {
            _channelId = channelId;
            _client = client;
        }

        public bool HasMoreItems => Page >= 0;
        public int Page { get; private set; } = 1;

        private bool isLoading;

        public bool IsLoading
        {
            get { return isLoading; }
            set { isLoading = value; this.OnPropertyChanged(new(nameof(IsLoading))); }
        }


        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            IsLoading = true;
            return Task.Run(async () =>
            {
                var messages = await _client.RequestChannelMessages(_channelId, Page);
                if (messages.Messages.Count > 0)
                    Page++;
                else
                    Page = -1;
                foreach (var message in messages.Messages)
                {
                    App.MainWindow.DispatcherQueue.TryEnqueue(() =>
                    {
                        this.Insert(0, message);
                    });
                }
                App.MainWindow.DispatcherQueue.TryEnqueue(() => IsLoading = false);
                return new LoadMoreItemsResult()
                {
                    Count = (uint)messages.Messages.Count
                };
            }).AsAsyncOperation<LoadMoreItemsResult>();
        }
    }
}
