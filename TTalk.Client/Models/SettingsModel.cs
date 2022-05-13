using Avalonia.Threading;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TTalk.Client.ViewModels;

namespace TTalk.Client.Models
{
    public class SettingsModel : BaseReactiveModel
    {

        private static SettingsModel instance;
        private static bool isInitialized;

        public static SettingsModel I
        {
            get
            {
                return instance;
            }
        }

        private SettingsModel()
        {
            InputDevices = new();
            OutputDevices = new();

            var enumerator = new MMDeviceEnumerator();
            int waveOutDevices = WaveOut.DeviceCount;
            for (int waveOutDevice = 0; waveOutDevice < waveOutDevices; waveOutDevice++)
            {
                WaveOutCapabilities deviceInfo = WaveOut.GetCapabilities(waveOutDevice);
                foreach (MMDevice device in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.All))
                {
                    if (device.FriendlyName.StartsWith(deviceInfo.ProductName))
                    {

                        OutputDevices.Add(device.FriendlyName);
                        break;
                    }
                }
            }

            if (WaveOut.DeviceCount > 0)
                OutputDevice = 0;

            int waveInDevices = WaveIn.DeviceCount;
            for (int waveInDevice = 0; waveInDevice < waveInDevices; waveInDevice++)
            {
                WaveInCapabilities deviceInfo = WaveIn.GetCapabilities(waveInDevice);
                foreach (MMDevice device in enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.All))
                {
                    if (device.FriendlyName.StartsWith(deviceInfo.ProductName))
                    {
                        InputDevices.Add(device.FriendlyName);
                        break;
                    }
                }

            }

            if (WaveIn.DeviceCount > 0)
                InputDevice = 0;
        }

        public string Version => "0.0.1";


        private string username;

        public string Username
        {
            get
            {
#if DEBUG
                if (!username.StartsWith("Random"))
                    username = "Random" + new Random().Next(0, 1000).ToString();
#endif
                return username;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref username, value);
            }
        }

        private ObservableCollection<string> outputDevices;
        [JsonIgnore]
        public ObservableCollection<string> OutputDevices
        {
            get { return outputDevices; }
            set { this.RaiseAndSetIfChanged(ref outputDevices, value); }
        }



        private ObservableCollection<string> inputDevices;
        [JsonIgnore]
        public ObservableCollection<string> InputDevices
        {
            get { return inputDevices; }
            set { this.RaiseAndSetIfChanged(ref inputDevices, value); }
        }

        private int outputDevice;

        public int OutputDevice
        {
            get { return outputDevice; }
            set
            {
                this.RaiseAndSetIfChanged(ref outputDevice, value,
                    onChanged: () =>
                    {
                        Task.Run(async () =>
                        {
                            if (Main.CurrentChannel == null) return;
                            var channelId = Main.CurrentChannel.Id.ToString();
                            Main.StopAudioPlayback();
                            Main.StopAudioPlayback();

                        });
                    });
            }
        }

        private int inputDevice;

        public int InputDevice
        {
            get { return inputDevice; }
            set
            {
                this.RaiseAndSetIfChanged(ref inputDevice, value, onChanged:
                  () =>
                  {
                      Task.Run(async () =>
                      {
                          if (Main.CurrentChannel == null) return;
                          var channelId = Main.CurrentChannel.Id.ToString();
                          Main.StopEncoding();
                          Main.StartEncoding(Main.CurrentChannel.Bitrate);

                      });
                  });
            }
        }

        private double threshold;

        public double Threshold
        {
            get { return threshold; }
            set { this.RaiseAndSetIfChanged(ref threshold, value); }
        }

        private bool useVoiceActivityDetection;

        public bool UseVoiceActivityDetection
        {
            get { return useVoiceActivityDetection; }
            set { this.RaiseAndSetIfChanged(ref useVoiceActivityDetection, value); }
        }
        private bool noiseReductionEnabled;

        public bool NoiseReductionEnabled
        {
            get { return noiseReductionEnabled; }
            set { this.RaiseAndSetIfChanged(ref noiseReductionEnabled, value); }
        }

        [JsonIgnore]
        public MainWindowViewModel Main { get; set; }
        public static void Init()
        {
            var _path = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            _path = System.IO.Path.Combine(_path, "TTalk");
            if (!Directory.Exists(_path))
                Directory.CreateDirectory(_path);
            _path = System.IO.Path.Combine(_path, "config.json");
            if (!File.Exists(_path))
            {
                instance = new SettingsModel();
                File.WriteAllText(_path, JsonConvert.SerializeObject(instance));
                isInitialized = true;
            }
            else
            {
                try
                {

                    instance = JsonConvert.DeserializeObject<SettingsModel>(File.ReadAllText(_path));
#if DEBUG
                    instance.username = "";
#endif
                    isInitialized = true;
                }
                catch
                {
                    instance = new SettingsModel();
                    File.WriteAllText(_path, JsonConvert.SerializeObject(instance));
                    isInitialized = true;
                }
            }

        }

        private void WriteSettings()
        {
            if (!isInitialized) return;
            var _path = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            _path = System.IO.Path.Combine(_path, "TTalk", "config.json");
            File.WriteAllText(_path, JsonConvert.SerializeObject(this));
        }

        public override bool RaiseAndSetIfChanged<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "", Action onChanged = null)
        {
            var res = base.RaiseAndSetIfChanged(ref backingStore, value, propertyName, onChanged);
            if (res)
                WriteSettings();
            return res;
        }


    }
}
