using DnsClient;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Remore.Library.Packets;
using Remore.WinUI.Activation;
using Remore.WinUI.Contracts.Services;
using Remore.WinUI.Core.Contracts.Services;
using Remore.WinUI.Core.Services;
using Remore.WinUI.Helpers;
using Remore.WinUI.Models;
using Remore.WinUI.Services;
using Remore.WinUI.ViewModels;
using Remore.WinUI.Views;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Resources.Core;
using Windows.Globalization;
using Windows.Storage;

// To learn more about WinUI3, see: https://docs.microsoft.com/windows/apps/winui/winui3/.
namespace Remore.WinUI
{
    public partial class App : Application
    {
        private static IHost _host = Host
            .CreateDefaultBuilder()
            .ConfigureLogging((context, logging) => {
                var env = context.HostingEnvironment;
                var config = context.Configuration.GetSection("Logging");
                logging.AddFile($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/Remore/logs/" + 
                    string.Format("app_{0:yyyy}-{0:MM}-{0:dd}-{0:hh}-{0:mm}.log", DateTime.Now), fileLoggerOpts => {
                });
                logging.AddConsole();
                logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", Microsoft.Extensions.Logging.LogLevel.None);
                logging.AddFilter("Microsoft.EntityFrameworkCore.Model.Validation", Microsoft.Extensions.Logging.LogLevel.None);
                logging.AddFilter("Microsoft.EntityFrameworkCore.Infrastructure", Microsoft.Extensions.Logging.LogLevel.None);
                logging.AddFilter("Microsoft.EntityFrameworkCore.Query", Microsoft.Extensions.Logging.LogLevel.None);
            })
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
                services.AddTransient<SettingsViewModel>((_) =>
                {
                    return _settingsViewModel;
                });
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

                services.AddSingleton<ILookupClient>(new LookupClient());
            })
            .Build();

        public static T GetService<T>()
            where T : class
            => _host.Services.GetService(typeof(T)) as T;

        public static Window MainWindow { get; set; }

        private static MainViewModel _mainViewModel;
        private static SettingsViewModel _settingsViewModel;
        private static Mutex mutex = new Mutex(true, "RemoreSINGLEINSTANCEAPPLICATIONMUTEX");

        public App()
        {
            GetService<ILogger<App>>().LogInformation($"Application launched");
            InitializeComponent();
            UnhandledException += App_UnhandledException;
            PacketReader.Init();
        }

        
        public static void ResetMainViewModel()
        {
            _mainViewModel = new(GetService<ILocalSettingsService>(), GetService<ILogger<MainViewModel>>(), GetService<SoundService>(), GetService<KeyBindingsService>(), GetService<ILookupClient>());
            _settingsViewModel = new SettingsViewModel(App.GetService<IThemeSelectorService>(),
                   App.GetService<ILogger<SettingsViewModel>>(),
                   GetService<KeyBindingsService>(),
                   GetService<LocalizationService>(),
                   GetService<MainViewModel>(),
                   GetService<ILocalSettingsService>());
        }

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            GetService<ILogger<App>>().LogCritical($"Application crashed");
            GetService<ILogger<App>>().LogCritical(e.Exception.Message);
            GetService<ILogger<App>>().LogCritical(e.Exception.StackTrace);
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
