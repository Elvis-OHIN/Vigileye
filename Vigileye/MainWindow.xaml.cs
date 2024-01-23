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
using Vigileye.Models;
using System.Net.NetworkInformation;
using System.Diagnostics;
using System.Management;
using System.Windows.Interop;
using System;
using System.Drawing;
using Image = System.Windows.Controls.Image;
using System.Windows.Threading;

namespace Vigileye
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Dictionary<string, PieChart> diskCharts = new Dictionary<string, PieChart>();


        private DispatcherTimer memoryUpdateTimer;

        private double gaugeValue = 0;
        private double gaugeMaxValue = 100;
        public MainWindow()
        {   
           
            InitializeComponent();
            InitializeMemoryUpdateTimer();
            LoadSystemInfo();
            ConfigureDisksComboBox();
            DisplayRAMInfo();
            GetNetworkInfo();
            GetRunningApplications();
            Closing += MainWindow_Closing;
            Visibility = Visibility.Hidden;

        }
        private void InitializeMemoryUpdateTimer()
        {
            memoryUpdateTimer = new DispatcherTimer();
            memoryUpdateTimer.Interval = TimeSpan.FromSeconds(3);
            memoryUpdateTimer.Tick += MemoryUpdateTimer_Tick;
            memoryUpdateTimer.Start();
        }
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Visibility = Visibility.Hidden;
        }

        private async void MemoryUpdateTimer_Tick(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    DisplayRAMInfo();
                    GetRunningApplications();
                });
            });
        }
        private void LoadSystemInfo()
        {
            txtComputerName.Text = $"Nom de l'ordinateur: {SystemInfo.GetComputerName()}";
            txtProcessorName.Text = $"Nom du processeur: {SystemInfo.GetProcessorName()}";
            txtSystemArchitecture.Text = $"Architecture du système: {SystemInfo.GetSystemArchitecture()}";
            txtOSInfo.Text = $"OS: {SystemInfo.GetOperatingSystemInfo()}";
            txtGPUInfo.Text = $"GPU: {SystemInfo.GetGPUName()}";
            txtMotherboardInfo.Text = $"Carte Mère: {SystemInfo.GetMotherboardInfo()}";
        }

        private void ConfigureDisksComboBox()
        {
            var storageData = SystemInfo.GetStorageInfo();
            foreach (var driveData in storageData)
            {
                // Ajouter le nom du disque à la ComboBox
                DisksComboBox.Items.Add(driveData.Key);

                // Créer et configurer le PieChart pour ce disque
                var chart = new PieChart
                {
                    Series = new SeriesCollection
            {
                new PieSeries
                {
                    Title = "Espace Utilisé",
                    Values = new ChartValues<long> { driveData.Value.Key },
                    DataLabels = true,
                    LabelPoint = chartPoint => $"{chartPoint.Y} GB"
                },
                new PieSeries
                {
                    Title = "Espace Libre",
                    Values = new ChartValues<long> { driveData.Value.Value },
                    DataLabels = true,
                    LabelPoint = chartPoint => $"{chartPoint.Y} GB"
                }
            }
                };

                // Ajouter le PieChart au dictionnaire
                diskCharts[driveData.Key] = chart;
            }

            // Sélectionner le premier élément par défaut
            if (DisksComboBox.Items.Count > 0)
                DisksComboBox.SelectedIndex = 0;
        }
        private void DisplayRAMInfo()
        {
            var (totalMemory, usedMemory) = SystemInfo.GetRAMInfo();
            var usedPercentage = (double)usedMemory / totalMemory * 100;

            RamUsageGauge.Value = usedPercentage; // Met à jour la jauge
            txtRamUsage.Text = $"RAM utilisée: {usedPercentage:0.0}% de {totalMemory} MB";
        }


        private void DisksComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DisksComboBox.SelectedItem != null)
            {
                string selectedDisk = DisksComboBox.SelectedItem.ToString();
                ChartContentControl.Content = diskCharts[selectedDisk]; // Mettre à jour le contenu
            }
        }
        private void GetNetworkInfo()
        {
            // Obtenir les informations sur l'interface réseau par défaut
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            NetworkInterface defaultInterface = null;
            foreach (var networkInterface in networkInterfaces)
            {
                if (networkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    defaultInterface = networkInterface;
                    break;
                }
            }

            if (defaultInterface != null)
            {
                // Obtenir les informations réseau
                var networkProperties = defaultInterface.GetIPProperties();
                var ipAddress = networkProperties.UnicastAddresses[0].Address;
                var subnetMask = networkProperties.UnicastAddresses[0].IPv4Mask;
                var defaultGateway = networkProperties.GatewayAddresses[0].Address;
                var dnsServers = networkProperties.DnsAddresses;

                // Afficher les informations récupérées dans des TextBox ou d'autres contrôles WPF
                ipAddressTextBox.Text = ipAddress.ToString();
                subnetMaskTextBox.Text = subnetMask.ToString();
                defaultGatewayTextBox.Text = defaultGateway.ToString();
                dnsServersTextBox.Text = string.Join(", ", dnsServers);
            }
            else
            {
                MessageBox.Show("Aucune interface réseau active trouvée.");
            }
        }
        private void GetRunningApplications()
        {
            // Efface la liste actuelle des applications visibles
            listBoxApps.Items.Clear();

            // Récupère la liste des processus en cours d'exécution
            Process[] processes = Process.GetProcesses();

            // Filtrer les processus pour les applications visibles
            var visibleApps = processes
                .Where(process => !string.IsNullOrEmpty(process.MainWindowTitle))
                .ToList();

            // Ajoute les noms des applications visibles à la ListBox
            foreach (var app in visibleApps)
            {
                try
                {
                    if (HasProcessAccess(app))
                    {
                        Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(app.MainModule.FileName);

                        if (icon != null)
                        {
                            BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHIcon(
                                icon.Handle,
                                Int32Rect.Empty,
                                BitmapSizeOptions.FromEmptyOptions());

                            Image appImage = new Image();
                            appImage.Source = bitmapSource;

                            StackPanel stackPanel = new StackPanel();
                            stackPanel.Orientation = Orientation.Horizontal;
                            stackPanel.Children.Add(appImage);

                            TextBlock textBlock = new TextBlock();
                            textBlock.Text = app.ProcessName;
                            textBlock.Margin = new Thickness(5, 0, 0, 0);
                            stackPanel.Children.Add(textBlock);

                            listBoxApps.Items.Add(stackPanel);
                        }
                    }
                }
                catch (System.ComponentModel.Win32Exception)
                {
                   
                }
                catch (Exception)
                {
                   
                }
            }
        }

        private bool HasProcessAccess(Process process)
        {
            try
            {
                int sessionId = process.SessionId;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }
            Visibility = Visibility.Visible;
            Activate();
            Topmost = true;
            Topmost = false;
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

    }
}