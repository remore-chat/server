using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTalk.Client.ViewModels;
using TTalk.Library.Enums;
using TTalk.Library.Models;

namespace TTalk.Client.Models
{
    public class Channel : INotifyPropertyChanged
    {
        private string id;

        public string Id
        {
            get { return id; }
            set { id = value; PropertyChanged?.Invoke(this, new(nameof(Id))); }
        }

        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; PropertyChanged?.Invoke(this, new(nameof(Name))); }
        }

        private int maxClients;

        public int MaxClients
        {
            get { return maxClients; }
            set { maxClients = value; PropertyChanged?.Invoke(this, new(nameof(maxClients))); }
        }

        private int clientsCount;

        public int ClientsCount
        {
            get { return clientsCount; }
            set { clientsCount = value; PropertyChanged?.Invoke(this, new(nameof(ClientsCount))); }
        }

        public int LastParsedPage { get; set; } = -1;

        private ObservableCollection<ChannelMessage> messages;

        public ObservableCollection<ChannelMessage> Messages
        {
            get { return messages; }
            set { messages = value; PropertyChanged?.Invoke(this, new(nameof(Messages))); }
        }



        private ObservableCollection<ChannelClient> connectedClients;

        public ObservableCollection<ChannelClient> ConnectedClients
        {
            get { return connectedClients; }
            set
            {
                connectedClients = value;
                value.CollectionChanged += (s, e) => ClientsCount = e.NewItems?.Count ?? 0;
                PropertyChanged?.Invoke(this, new(nameof(ConnectedClients)));
            }
        }

        private int bitrate;

        public int Bitrate
        {
            get { return bitrate; }
            set
            {
                bitrate = value;
                PropertyChanged?.Invoke(this, new(nameof(Bitrate)));
            }
        }


        private ChannelType channelType;

        public ChannelType СhannelType
        {
            get { return channelType; }
            set { channelType = value; PropertyChanged?.Invoke(this, new(nameof(СhannelType))); }
        }


        public void JoinChannel(object parameter)
        {
            var id = (string)parameter;
            Task.Run(() => Parent.JoinChannel(this));
        }

        public MainWindowViewModel Parent { get; internal set; }
        public int Order { get; internal set; }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
