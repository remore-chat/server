using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Remore.WinUI.ViewModels
{
    public partial class ServerSettingsViewModel : ObservableObject, ICloneable
    {
        private Action<string, int> _updateMethod;

        [ObservableProperty]
        [AlsoNotifyChangeFor(nameof(CanExecute))]
        private string name;

        [ObservableProperty]
        [AlsoNotifyChangeFor(nameof(CanExecute))]
        private int maxClients;


        public bool CanExecute
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Name) || Name.Length > 64)
                    return false;
                if (MaxClients <= 0 || MaxClients >= 256000)
                    return false;
                return true;
            }
        }

        //TODO: update action to func and notify client whether the info was updater or not; after decoupling
        public ServerSettingsViewModel()
        {
        }
        public void Init(Action<string, int> updateMethod, string name, int maxClients)
        {
            _updateMethod = updateMethod;
            Name = name;
            MaxClients = maxClients;
        }

        public RelayCommand SaveServerInfoCommand { get; }

        public static bool IsSettingsChanged(ServerSettingsViewModel obj1, ServerSettingsViewModel obj2)
        {
            if (obj1 == null || obj2 == null)
                return true;
            if (obj1.MaxClients != obj2.MaxClients)
                return true;
            if (obj1.Name != obj2.Name)
                return true;
            return false;
        }

        public void SaveServerInfo()
        {
            _updateMethod(Name, MaxClients);
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
