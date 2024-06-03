
using LiveCharts.Wpf;
using LiveCharts;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.NetworkInformation;
using System.Diagnostics;
using System.Management;
using System.Windows.Interop;
using System;
using System.Drawing;
using Image = System.Windows.Controls.Image;
using System.Windows.Threading;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.IO;
using Microsoft.VisualBasic.Devices;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Net.Http.Json;
using Microsoft.AspNetCore.SignalR.Client;
using SynetraUtils.Models.MessageManagement;
using Microsoft.Toolkit.Uwp.Notifications;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;
using WindowsInput;
using WindowsInput.Native;
using Windows.Media.Protection.PlayReady;
using System.Text.Json;
using Vigileye.Models;
namespace Vigileye
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Déclaration des variables de classe pour les paramètres de l'application et le chemin du fichier de paramètres
        private AppSettings _settings;
        private string _appSettingsPath;

        // Constructeur de la fenêtre principale
        public MainWindow()
        {
            InitializeComponent(); // Initialisation des composants de la fenêtre
            LoadSettings(); // Chargement des paramètres de l'application
            DataContext = _settings; // Définir le contexte de données de la fenêtre aux paramètres de l'application
            Closing += MainWindow_Closing; // Abonnement à l'événement de fermeture de la fenêtre
            Visibility = Visibility.Hidden; // Masquer la fenêtre au démarrage
        }

        // Méthode pour charger les paramètres de l'application
        private void LoadSettings()
        {
            var app = (App)System.Windows.Application.Current;
            _settings = app.Settings; // Récupération des paramètres de l'application depuis l'instance de l'application

            var exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var exeDir = System.IO.Path.GetDirectoryName(exePath);
            _appSettingsPath = System.IO.Path.Combine(exeDir, "appsettings.json"); // Construction du chemin complet du fichier de paramètres
        }

        // Événement de fermeture de la fenêtre
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true; // Annuler la fermeture de la fenêtre
            Visibility = Visibility.Hidden; // Masquer la fenêtre au lieu de la fermer
        }

        // Gestionnaire de clic pour le bouton d'ouverture de la fenêtre
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal; // Restaurer la fenêtre si elle est minimisée
            }
            Visibility = Visibility.Visible; // Rendre la fenêtre visible
            Activate(); // Activer la fenêtre
            Topmost = true; // Mettre la fenêtre au premier plan temporairement
            Topmost = false; // Réinitialiser la propriété Topmost
        }

        // Méthode pour sauvegarder les paramètres de l'application
        private void SaveSettings()
        {
            var settings = new
            {
                AppConfig = new
                {
                    Url = _settings.URL,
                    ParcId = _settings.ParcId,
                    RoomId = _settings.RoomId
                }
            };

            var options = new JsonSerializerOptions { WriteIndented = true }; // Options pour la sérialisation JSON
            string jsonString = JsonSerializer.Serialize(settings, options); // Sérialiser les paramètres en JSON
            File.WriteAllText(_appSettingsPath, jsonString); // Écrire le JSON dans le fichier
        }

        // Gestionnaire de clic pour le bouton de sauvegarde
        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings(); // Sauvegarder les paramètres de l'application
        }

        // Gestionnaire de clic pour le bouton de sortie
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown(); // Fermer l'application
        }
    }
}