using LiveCharts.Wpf;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using System.Data;
using System.IO;
using System.Security.Policy;
using System.Windows;
using Vigileye.Models;
using Vigileye.Services.SignalR;

namespace Vigileye
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IConfiguration Configuration { get; private set; }
        public SignalRService signalRService { get; private set; }
        public string Url { get; private set; } 

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Chargement de la configuration
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            Url = Configuration["AppConfig:Url"];

            // Initialisation du SignalRService avec la configuration
            signalRService = new SignalRService();
            signalRService.StartAsync();
        }
    }

}
