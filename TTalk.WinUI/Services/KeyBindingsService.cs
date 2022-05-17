using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TTalk.WinUI.Contracts.Services;
using TTalk.WinUI.KeyBindings;
using TTalk.WinUI.ViewModels;

namespace TTalk.WinUI.Services
{
    public class KeyBindingsService
    {
        private readonly ILocalSettingsService _localSettingsService;
        private List<KeyBinding> _keyBindings;
        private Timer timer;
        private const short WM_KEYDOWN = short.MinValue;
        private const short WM_KEYUP = 0;

        public KeyBindingsService(ILocalSettingsService localSettingsService)
        {
            _localSettingsService = localSettingsService;
        }

        public IReadOnlyList<KeyBinding> KeyBindings => _keyBindings;

        public event EventHandler<KeyBindingExecutedEventArgs> KeyBindingExecuted;
        public bool IsKeyBindingsEnabled { get; set; }

        public async Task Initialize()
        {
            _keyBindings = new();
            var keyBindingsSettings = await _localSettingsService.ReadSettingAsync<List<KeyBinding>>(SettingsViewModel.KeyBindingsListSettingsKey);
            if (keyBindingsSettings != null)
            {
                _keyBindings = keyBindingsSettings;
            }
            IsKeyBindingsEnabled = true;
            timer = new Timer((state) => ListenKeyboard(), null, 0, 65);
        }

        public bool RegisterKeyBinding(KeyBinding keyBinding)
        {
            if (KeyBindings.Any(x => x.Key == keyBinding.Key))
                return false;
            _keyBindings.Add(keyBinding);
            Task.Run(async () => await _localSettingsService.SaveSettingAsync<List<KeyBinding>>(SettingsViewModel.KeyBindingsListSettingsKey, _keyBindings));
            return true;
        }

        public bool RemoveKeyBinding(PInvoke.User32.VirtualKey keyBindingKey)
        {
            var keyBinding = KeyBindings.FirstOrDefault(x => x.Key == keyBindingKey);
            if (keyBinding == null)
                return false;
            _keyBindings.Remove(keyBinding);
            Task.Run(async () => await _localSettingsService.SaveSettingAsync<List<KeyBinding>>(SettingsViewModel.KeyBindingsListSettingsKey, _keyBindings));
            return true;
        }

        public void ListenKeyboard()
        {
            if (!IsKeyBindingsEnabled)
                return;
            // Convertion to list to avoid collection was modified exception
            Parallel.ForEach(_keyBindings.ToList(), async (binding) =>
            {
                var key = PInvoke.User32.GetAsyncKeyState((int)binding.Key);
                if (PInvoke.User32.GetAsyncKeyState((int)binding.Key) == WM_KEYDOWN)
                {
                    // Check if key was pressed and released immediately
                    await Task.Delay(65);
                    if (PInvoke.User32.GetAsyncKeyState((int)binding.Key) == WM_KEYUP)
                    {
                        App.MainWindow.DispatcherQueue.TryEnqueue(() =>
                            KeyBindingExecuted?.Invoke(this, new() { KeyBinding = binding }));
                    }
                }
            });
        }

    }

    public class KeyBindingExecutedEventArgs
    {
        public KeyBinding KeyBinding { get; set; }
    }
}
