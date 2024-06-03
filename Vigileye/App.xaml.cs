using LiveCharts.Wpf;
using Microsoft.Extensions.Configuration;
using SynetraUtils.Models.DataManagement;
using System.Configuration;
using System.Data;
using System.IO;
using System.Security.Policy;
using System.Windows;
using Vigileye.Services.SignalR;
using Vigileye.Models;

namespace Vigileye
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // Propriétés pour la configuration, le service SignalR, l'URL, les paramètres et les identifiants de parc et de salle
        public IConfiguration Configuration { get; private set; }
        public SignalRService signalRService { get; private set; }
        public string Url { get; private set; }
        public AppSettings Settings { get; private set; }
        public int Parc { get; private set; }
        public int Room { get; private set; }

        // Méthode exécutée au démarrage de l'application
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Chargement de la configuration à partir du fichier JSON
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Initialiser les propriétés à partir de la configuration
            Url = Configuration["AppConfig:Url"];
            Parc = Convert.ToInt32(Configuration["AppConfig:ParcId"]);
            Room = Convert.ToInt32(Configuration["AppConfig:RoomId"]);

            // Charger les paramètres de configuration dans l'objet Settings
            Settings = new AppSettings
            {
                URL = Url,
                ParcId = Parc,
                RoomId = Room
            };

            // Démarrer le service SignalR de manière asynchrone
            signalRService = new SignalRService();
            await signalRService.StartAsync();
        }
    }
}
