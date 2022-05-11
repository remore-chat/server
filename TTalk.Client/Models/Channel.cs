using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTalk.Client.ViewModels;

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


        public void JoinChannel(object parameter)
        {
            var id = (string)parameter;
            Task.Run(() => Parent.JoinChannel(id));
        }

        public MainWindowViewModel Parent { get; internal set; }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
