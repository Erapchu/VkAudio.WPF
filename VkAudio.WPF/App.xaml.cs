using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System;
using System.Threading;
using System.Windows;
using VkAudio.WPF.ViewModels;
using VkAudio.WPF.Views;

namespace VkAudio.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly IConfiguration _configuration;
        private readonly Mutex _mutex;
        private ILogger<App> _logger;

        private bool IsFirstInstance { get; }
        public IServiceProvider Services { get; }

        public App()
        {
            _mutex = new Mutex(true, "VkAudio_9279939C-3E47-43B5-A797-DB4D5BFB9059", out bool isFirstInstance);
            IsFirstInstance = isFirstInstance;

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;

            if (IsFirstInstance)
            {
                _configuration = BuildConfiguration();
                Services = ConfigureServices(_configuration);
            }
        }

        private static IServiceProvider ConfigureServices(IConfiguration configuration)
        {
            var services = new ServiceCollection();

            services.AddOptions();

            // NLog
            services.AddLogging(lb =>
            {
                lb.ClearProviders();
                lb.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                lb.AddNLog(configuration);
            });

            services.AddHttpClient();

            // Windows
            services.AddScoped<MainWindow>();
            services.AddScoped<MainWindowViewModel>();
            services.AddTransient<MaterialInputBoxContent>();
            services.AddTransient<MaterialInputBoxViewModel>();

            return services.BuildServiceProvider();
        }

        private static IConfiguration BuildConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("settings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            InitializeComponent();

            if (IsFirstInstance)
            {
                _logger = Services.GetService<ILogger<App>>();
                _logger.LogInformation("Log session started!");

                var mainWindow = Services.GetService<MainWindow>();
                mainWindow.Show();
            }
            else
            {
                Shutdown();
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            _logger?.LogInformation("The application is shutting down...{0}", Environment.NewLine);
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            _logger?.LogError(e.Exception, "Dispatcher unhandled exception");
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger?.LogError(e.ExceptionObject as Exception, "Domain unhandled exception");
        }
    }
}
