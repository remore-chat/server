using Avalonia.Threading;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace TTalk.Client.Models
{
    public class ChannelClient : INotifyPropertyChanged
    {
        private string username;

        public string Username
        {
            get { return username; }
            set { username = value; PropertyChanged?.Invoke(this, new(nameof(Username))); }
        }
        private bool isSpeaking;

        public bool IsSpeaking
        {
            get { return isSpeaking; }
            set
            {
                if (value == isSpeaking)
                    return;
                isSpeaking = value;
                PropertyChanged?.Invoke(this, new(nameof(IsSpeaking)));
            }
        }

        private bool isMuted;

        public bool IsMuted
        {
            get { return isMuted; }
            set { isMuted = value; PropertyChanged?.Invoke(this, new(nameof(IsMuted))); }
        }


        private Timer _timer;

        public long LastTimeVoiceDataReceived { get; set; }

        public ChannelClient(string username)
        {
            Username = username;
            IsSpeaking = false;
            _timer = new Timer((state) =>
            {
                if (DateTimeOffset.Now.ToUnixTimeSeconds() - LastTimeVoiceDataReceived > 1)
                    IsSpeaking = false;
            }, null, 0, 1000);
        }


        public event PropertyChangedEventHandler? PropertyChanged;
    }
}