using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Remore.Library.Enums;
using Remore.Library.Models;
using Remore.WinUI.ViewModels;
using Remore.WinUI.Controls;

namespace Remore.WinUI.Models
{
    public class Channel : ObservableRecipient
    {

        public Channel()
        {
            JoinChannel = new RelayCommand(() =>
            {
                App.MainWindow.DispatcherQueue.TryEnqueue(async () => await Parent.JoinChannel(this));
            });
        }

        private string id;

        public string Id
        {
            get { return id; }
            set { SetProperty(ref id, value); }
        }

        private string name;

        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }

        private int maxClients;

        public int MaxClients
        {
            get { return maxClients; }
            set { if (SetProperty(ref maxClients, value))
                {
                    HasUserLimit = value != 999999;
                }
            }
        }

        private int clientsCount;

        public int ClientsCount
        {
            get { return clientsCount; }
            set { SetProperty(ref clientsCount, value); }
        }

        public int LastParsedPage { get; set; } = -1;

        private Conversation messages;

        public Conversation Messages
        {
            get
            {
                return messages;
            }
            set { SetProperty(ref messages, value); }
        }



        private ObservableCollection<ChannelClient> connectedClients;

        public ObservableCollection<ChannelClient> ConnectedClients
        {
            get { return connectedClients; }
            set
            {
                SetProperty(ref connectedClients, value);
            }
        }

        private int bitrate;

        public int Bitrate
        {
            get { return bitrate; }
            set
            {
                SetProperty(ref bitrate, value);
            }
        }

        private bool isSelected;

        public bool IsSelected
        {
            get { return isSelected; }
            set { SetProperty(ref isSelected, value); }
        }

        private bool hasUserLimit;

        public bool HasUserLimit
        {
            get { return hasUserLimit; }
            set { SetProperty(ref hasUserLimit, value); }
        }


        private ChannelType channelType;

        public ChannelType ChannelType
        {
            get { return channelType; }
            set { SetProperty(ref channelType, value); }
        }

        public bool IsText => ChannelType == ChannelType.Text;

        public MainViewModel Parent { get; internal set; }
        public int Order { get; internal set; }
        public ICommand JoinChannel { get; }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
