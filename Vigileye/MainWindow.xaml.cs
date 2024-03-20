
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
        private bool isCapturing = false;
        private Thread captureThread;
        private HubConnection hubConnection;
        Dictionary<string, VirtualKeyCode> keyMappings = new Dictionary<string, VirtualKeyCode>()
        {
            {"a", VirtualKeyCode.VK_A},
            {"b", VirtualKeyCode.VK_B},
            {"c", VirtualKeyCode.VK_C},
            {"d", VirtualKeyCode.VK_D},
            {"e", VirtualKeyCode.VK_E},
            {"f", VirtualKeyCode.VK_F},
            {"g", VirtualKeyCode.VK_G},
            {"h", VirtualKeyCode.VK_H},
            {"i", VirtualKeyCode.VK_I},
            {"j", VirtualKeyCode.VK_J},
            {"k", VirtualKeyCode.VK_K},
            {"l", VirtualKeyCode.VK_L},
            {"m", VirtualKeyCode.VK_M},
            {"n", VirtualKeyCode.VK_N},
            {"o", VirtualKeyCode.VK_O},
            {"p", VirtualKeyCode.VK_P},
            {"q", VirtualKeyCode.VK_Q},
            {"r", VirtualKeyCode.VK_R},
            {"s", VirtualKeyCode.VK_S},
            {"t", VirtualKeyCode.VK_T},
            {"u", VirtualKeyCode.VK_U},
            {"v", VirtualKeyCode.VK_V},
            {"w", VirtualKeyCode.VK_W},
            {"x", VirtualKeyCode.VK_X},
            {"y", VirtualKeyCode.VK_Y},
            {"z", VirtualKeyCode.VK_Z},

            {"ArrowUp", VirtualKeyCode.UP},
            {"ArrowDown", VirtualKeyCode.DOWN},
            {"ArrowLeft", VirtualKeyCode.LEFT},
            {"ArrowRight", VirtualKeyCode.RIGHT},

            {"Escape", VirtualKeyCode.ESCAPE},
            {"Enter", VirtualKeyCode.RETURN},
            {"Tab", VirtualKeyCode.TAB},
            {"Space", VirtualKeyCode.SPACE},
            {"Backspace", VirtualKeyCode.BACK},
            {"Delete", VirtualKeyCode.DELETE},
            {"Insert", VirtualKeyCode.INSERT},
            {"Home", VirtualKeyCode.HOME},
            {"End", VirtualKeyCode.END},
            {"PageUp", VirtualKeyCode.PRIOR},
            {"PageDown", VirtualKeyCode.NEXT},
            {"LeftShift", VirtualKeyCode.LSHIFT},
            {"RightShift", VirtualKeyCode.RSHIFT},
            {"LeftControl", VirtualKeyCode.LCONTROL},
            {"RightControl", VirtualKeyCode.RCONTROL},
            {"LeftMenu", VirtualKeyCode.LMENU}, // Alt key
            {"RightMenu", VirtualKeyCode.RMENU}, // Alt key
            
        };
        public MainWindow()
        {   
           
            InitializeComponent();
            /*InitializeMemoryUpdateTimer();
            LoadSystemInfo();
            ConfigureDisksComboBox();
            DisplayRAMInfo();
            GetNetworkInfo();
            GetRunningApplications();*/
            Connection();
            Closing += MainWindow_Closing;
            Visibility = Visibility.Hidden;
            CaptureDesktopImage();
            

        }
        private void Connection()
        {
            hubConnection = new HubConnectionBuilder()
              .WithUrl("https://localhost:7057/sharehub")
              .Build();
            hubConnection.On<string, string>("ReceiveMessage", (title, message) =>
            {
              
                Dispatcher.Invoke(() =>
                {
                    new ToastContentBuilder()
                       .AddArgument("action", "viewConversation")
                       .AddArgument("conversationId", 9813)
                       .AddText(title)
                       .AddText(message)
                       .Show();
                });
            });
            /*hubConnection.On<double, double>("ReceiveMouseMovement", (x, y) =>
            {
                // Convertir x et y en coordonnées absolues si nécessaire
                // Simuler le mouvement de la souris en WPF
                var inputSimulator = new InputSimulator();
                inputSimulator.Mouse.MoveMouseTo(x * 65535, y * 65535);
            });*/
            hubConnection.On<string>("ReceiveKeyPress", (key) =>
            {
                var inputSimulator = new InputSimulator();
                var keyCode = ConvertToVirtualKeyCode(key);
                if (keyCode != VirtualKeyCode.F19) // Assurez-vous de ne pas simuler 'NONE'
                {
                    inputSimulator.Keyboard.KeyPress(keyCode);
                }
            });
            var inputSimulator = new InputSimulator();
            hubConnection.StartAsync().Wait();
            var largeurServeur = Screen.PrimaryScreen.Bounds.Width;
            var hauteurServeur = Screen.PrimaryScreen.Bounds.Height;

            // Calculer les facteurs d'échelle
            double facteurEchelleX = (double)largeurServeur / 1200;
            double facteurEchelleY = (double)hauteurServeur / 350;

            // Adapter les coordonnées
            int xServeur = (int)(560 * facteurEchelleX);
            int yServeur = (int)(48 * facteurEchelleY);

            var xVirtual = ConvertToVirtualDesktopCoordinate(1, 1200);
            var yVirtual = ConvertToVirtualDesktopCoordinate(1, 350);

            inputSimulator.Mouse.MoveMouseToPositionOnVirtualDesktop(xVirtual, yVirtual);

        
            xVirtual = ConvertToVirtualDesktopCoordinate(1150, 1200);
            yVirtual = ConvertToVirtualDesktopCoordinate(330, 350);

            inputSimulator.Mouse.MoveMouseToPositionOnVirtualDesktop(xVirtual, yVirtual);
        }
        private static double ConvertToVirtualDesktopCoordinate(int pixelCoordinate, int screenSize)
        {
            // Le bureau virtuel de Windows a une échelle de 0 à 65535, indépendamment de la résolution de l'écran
            return (65535.0 * pixelCoordinate) / screenSize;
        }

        public VirtualKeyCode ConvertToVirtualKeyCode(string key)
        {
            if (keyMappings.TryGetValue(key, out VirtualKeyCode keyCode))
            {
                return keyCode;
            }
            else
            {
                // Gérer le cas où la touche n'est pas trouvée, éventuellement en loggant une erreur
                return VirtualKeyCode.F19; // NONE est un exemple, ajustez selon votre gestion d'erreur
            }
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
                    CaptureDesktopImage();
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
                Mac.Text = SystemInfo.ObtenirAdresseMac();
            }
            else
            {
                System.Windows.MessageBox.Show("Aucune interface réseau active trouvée.");
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
                            stackPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;
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
        private void CaptureDesktopImage()
        {
             isCapturing = true;
             captureThread = new Thread(CaptureScreenContinuously);
             captureThread.Start();
        }
        private void CaptureScreenContinuously()
        {
            while (isCapturing)
            {
                // Capture the screen
                double screenWidth = SystemParameters.PrimaryScreenWidth;
                double screenHeight = SystemParameters.PrimaryScreenHeight;

                // Create a bitmap with the screen dimensions
                Bitmap screenshot = new Bitmap((int)screenWidth, (int)screenHeight);

                // Capture the screen content
                using (Graphics g = Graphics.FromImage(screenshot))
                {
                    g.CopyFromScreen(new System.Drawing.Point(0, 0), System.Drawing.Point.Empty, new System.Drawing.Size((int)screenWidth, (int)screenHeight));
                }

                // Display the captured screenshot
                this.Dispatcher.Invoke(() =>
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = new System.IO.MemoryStream(ImageToByteArray(screenshot));
                    bitmapImage.EndInit();
                    ImageMessage file = new ImageMessage();

                    byte[] imageBytes = null;

                    if (bitmapImage != null)
                    {
                        // Créer un encodeur JPEG
                        JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(bitmapImage));

                        // Créer un MemoryStream pour stocker les données encodées
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            // Encoder l'image dans le MemoryStream
                            encoder.Save(memoryStream);

                            // Obtenir le tableau de bytes résultant
                            imageBytes = memoryStream.ToArray();
                        }
                    }
                    file.ImageBinary = imageBytes;
                    file.ImageHeaders = "data:" + "image/png" + ";base64,";
                    hubConnection.InvokeAsync("ImageMessage", file);
                });

                // Delay for a short period before capturing the next screen (adjust as needed)
                Thread.Sleep(1); // Capture approximately 10 frames per second
            }
        }
        private byte[] ImageToByteArray(Bitmap image)
        {
            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            image.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
            return stream.ToArray();
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
            System.Windows.Application.Current.Shutdown();
        }

    }
}