using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using TTalk.Library.Packets;
using TTalk.WinUI.Activation;
using TTalk.WinUI.Contracts.Services;
using TTalk.WinUI.Core.Contracts.Services;
using TTalk.WinUI.Core.Services;
using TTalk.WinUI.Helpers;
using TTalk.WinUI.Models;
using TTalk.WinUI.Services;
using TTalk.WinUI.ViewModels;
using TTalk.WinUI.Views;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Resources.Core;
using Windows.Globalization;

// To learn more about WinUI3, see: https://docs.microsoft.com/windows/apps/winui/winui3/.
namespace TTalk.WinUI
{
    public partial class App : Application
    {
        // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
        // https://docs.microsoft.com/dotnet/core/extensions/generic-host
        // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
        // https://docs.microsoft.com/dotnet/core/extensions/configuration
        // https://docs.microsoft.com/dotnet/core/extensions/logging
        private static IHost _host = Host
            .CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Default Activation Handler
                services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

                // Other Activation Handlers

                // Services
                try
                {
                    var desc = Package.Current.Id.Version;
                    services.AddSingleton<ILocalSettingsService, LocalSettingsServicePackaged>();
                }
                catch (Exception)
                {
                    services.AddSingleton<ILocalSettingsService, LocalSettingsServiceUnpackaged>();
                }
                services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
                services.AddTransient<INavigationViewService, NavigationViewService>();

                services.AddSingleton<IActivationService, ActivationService>();
                services.AddSingleton<IPageService, PageService>();
                services.AddSingleton<INavigationService, NavigationService>();

                // Core Services
                services.AddSingleton<IFileService, FileService>();
                services.AddSingleton<SoundService>();
                services.AddSingleton<KeyBindingsService>();
                // Views and ViewModels
                services.AddTransient<SettingsViewModel>();
                services.AddTransient<SettingsViewModel>();
                services.AddSingleton<LocalizationService>();
                services.AddTransient<MainViewModel>((_) =>
                {
                    if (_mainViewModel == null)
                        ResetMainViewModel();
                    return _mainViewModel;
                });
                services.AddTransient<MainPage>();
                services.AddTransient<ShellPage>();
                services.AddTransient<ShellViewModel>();

                // Configuration
                services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));


            })
            .Build();

        public static T GetService<T>()
            where T : class
            => _host.Services.GetService(typeof(T)) as T;

        public static Window MainWindow { get; set; }

        private static MainViewModel _mainViewModel;
        private static Mutex mutex = new Mutex(true, "TTALKSINGLEINSTANCEAPPLICATIONMUTEX");

        public App()
        {
            AppDomain.CurrentDomain.AssemblyResolve += LoadFromCurrentFolder;
            InitializeComponent();
            UnhandledException += App_UnhandledException;
            PacketReader.Init();
        }

        private System.Reflection.Assembly LoadFromCurrentFolder(object sender, ResolveEventArgs args)
        {
            string name = args.Name;

            bool bCheckVersion = false;
            int idx = name.IndexOf(',');
            if (idx != -1)
            {
                name = name.Substring(0, idx);
                bCheckVersion = true;
            }

            string sCurrentDir = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);

            if (!name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) && !name.EndsWith(".exe"))
            {
                string[] exts = { ".dll", ".exe" };
                foreach (string ext in exts)
                {
                    string tryPath = Path.Combine(sCurrentDir, name + ext);
                    if (File.Exists(tryPath))
                    {
                        name = name += ext;
                        break;
                    }
                }
            }

            string path = Path.Combine(sCurrentDir, name);
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                Assembly assembly = Assembly.LoadFrom(path);
                if (assembly != null & bCheckVersion)
                {
                    if (assembly.FullName != args.Name)
                        return null;
                }
                return assembly;
            }
            else
            {
                var reqAsm = args.RequestingAssembly;

                if (reqAsm != null)
                {
                    string requestingName = reqAsm.GetName().FullName;
                    Console.WriteLine($"Could not resolve {name}, {path}, requested by {requestingName}");
                }
                else
                {
                    Console.WriteLine($"Could not resolve {args.Name}, {path}");
                }
            }
            return null;
        }

        public static void ResetMainViewModel()
        {
            _mainViewModel = new(GetService<ILocalSettingsService>(), GetService<SoundService>(), GetService<KeyBindingsService>());
        }

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            // TODO: Log and handle exceptions as appropriate.
            // For more details, see https://docs.microsoft.com/windows/winui/api/microsoft.ui.xaml.unhandledexceptioneventargs.
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            await App.GetService<KeyBindingsService>().Initialize();
            await App.GetService<LocalizationService>().Initialize();
            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                System.Windows.Forms.MessageBox.Show("Application_Already_Running".GetLocalized());
                Environment.Exit(0);
                return;
            }

            MainWindow = new Window() { Title = "AppDisplayName".GetLocalized() };
            MainWindow.SetIcon("appicon.ico");
            base.OnLaunched(args);
            var activationService = App.GetService<IActivationService>();
            await activationService.ActivateAsync(args);

        }
    }
}
