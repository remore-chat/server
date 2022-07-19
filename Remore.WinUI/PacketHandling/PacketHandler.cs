using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Remore.Library.Packets;
using Remore.WinUI.ViewModels;

namespace Remore.WinUI.PacketHandling
{
    public class PacketHandler
    {
        private Dictionary<string, MethodInfo> _handlers;
        private ILogger<PacketHandler> _logger;
        private MainViewModel _viewModel;

        public PacketHandler(MainViewModel viewModel)
        {
            var mainViewModelType = typeof(MainViewModel);
            _handlers = Assembly.GetAssembly(typeof(IPacket)).GetTypes().Where(x => !x.IsInterface && typeof(IPacket).IsAssignableFrom(x))
               .ToDictionary(k => k.Name, v => mainViewModelType.GetMethod($"Handle{v.Name}"));
            _logger = App.GetService<ILogger<PacketHandler>>();
            _viewModel = viewModel;
        }

        public async Task HandlePacket(IPacket packet)
        {
            var name = packet.GetType().Name;
            var method = _handlers[name];
            if (method == null)
                _logger.LogWarning($"Method for handling packet {name} doesn't exists");
            else
            {
                try
                {
                    App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
                    {
                        var task = (Task)method.Invoke(_viewModel, new[] { packet });
                        await task;
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Exception while handling packet {name}\n{ex}");
                }
            }
        }
    }
}
