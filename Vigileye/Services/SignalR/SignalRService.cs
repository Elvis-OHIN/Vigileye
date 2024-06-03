using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Toolkit.Uwp.Notifications;
using SynetraUtils.Models.DataManagement;
using SynetraUtils.Models.MessageManagement;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Vigileye.Services.RemoteControl;
using Vigileye.SystemManager;

namespace Vigileye.Services.SignalR
{
    public class SignalRService
    {
        private readonly HubConnection hubConnection;
        private readonly InputSimulationService inputSimulationService;
        private bool isCapturing = false;
        private Thread captureThread;
        string url = string.Empty;
        int parc = 0;
        int room = 0;

        public SignalRService()
        {
            
            // Récupération de l'URL du serveur SignalR depuis les paramètres de l'application
            url = ((App)Application.Current).Url;
                parc = ((App)Application.Current).Parc;
                room = ((App)Application.Current).Room;

            // Initialisation de la connexion HubConnection avec l'URL et les paramètres de connexion
            hubConnection = new HubConnectionBuilder()
                    .WithUrl($"{url}/sharehub?footPrint={SystemInfo.GenerateHardwareId().Data}&key={SystemInfo.GenerateHardwareId().Key}&iv={SystemInfo.GenerateHardwareId().IV}")
                    .WithAutomaticReconnect()
                    .Build();

            // Initialisation du service de simulation d'entrées
            inputSimulationService = new InputSimulationService();

            // Configuration des gestionnaires d'événements pour la connexion Hub
            ConfigureEventHandlers();
           
        }

        private void ConfigureEventHandlers()
        {
            try
            {
                // Gestion des événements de la connexion Hub
                hubConnection.On<string>("ReceiveKeyPress", key =>
                {
                    try
                    {
                        inputSimulationService.SimulateKeyPress(key);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erreur lors de la simulation de la pression de touche : {ex.Message}");
                    }
                });

                hubConnection.On<string>("ReceiveClickPress", clic =>
                {
                    try
                    {
                        inputSimulationService.SimulateClickPress(clic);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erreur lors de la simulation de la pression de clic : {ex.Message}");
                    }
                });

                hubConnection.On<double, double, double, double>("ReceiveMouseMovement", (x, y, height, width) =>
                {
                    try
                    {
                        inputSimulationService.SimulateMouseMovement(x, y, height, width);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erreur lors de la simulation du mouvement de la souris : {ex.Message}");
                    }
                });

                hubConnection.On<string>("ReceiveDataOfPc", clic =>
                {
                    try
                    {
                        Computer computer = new Computer
                        {
                            CarteMere = SystemInfo.GetMotherboardInfo(),
                            GPU = SystemInfo.GetGPUName(),
                            Os = SystemInfo.GetOperatingSystemInfo(),
                            Name = SystemInfo.GetComputerName(),
                            IDProduct = SystemInfo.GetWindowsProductId(),
                            FootPrint = SystemInfo.GenerateHardwareId().Data,
                            ParcId = parc,
                            RoomId = room,
                            Statut = true,
                            IsActive = true,
                            OperatingSystem = SystemInfo.GetSystemArchitecture()
                        };
                        hubConnection.SendAsync("SendInfoOfPc", computer);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erreur lors de l'envoi des informations du PC : {ex.Message}");
                    }
                });

                hubConnection.On<string, string>("ReceiveMessage", (title, message) =>
                {
                    try
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            new ToastContentBuilder()
                               .AddArgument("action", "viewConversation")
                               .AddArgument("conversationId", 9813)
                               .AddText(title)
                               .AddText(message)
                               .Show();
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erreur lors de la réception du message : {ex.Message}");
                    }
                });

                hubConnection.On<string>("ReceiveModeVeille", key =>
                {
                    try
                    {
                        PowerManager.Sleep(hibernate: false, force: false, disableWakeEvent: false);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erreur lors de l'activation du mode veille : {ex.Message}");
                    }
                });

                hubConnection.On<string>("ReceiveModeOff", key =>
                {
                    try
                    {
                        PowerManager.Shutdown(true);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erreur lors de l'arrêt du système : {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la configuration des gestionnaires d'événements : {ex.Message}");
            }
        }

        public async Task SendNetworkInfo()
        {
            try
            {
                NetworkInfo networkInfo = new NetworkInfo();

                var defaultInterface = SystemInfo.GetPrimaryNetworkInterface();

                if (defaultInterface != null)
                {
                    var unicastIPAddressInformation = defaultInterface.GetIPProperties().UnicastAddresses
                        .FirstOrDefault(ip => ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                    if (unicastIPAddressInformation != null)
                    {
                        var ipAddress = unicastIPAddressInformation.Address;
                        var subnetMask = unicastIPAddressInformation.IPv4Mask;
                        var defaultGateway = SystemInfo.GetBroadcastAddress(unicastIPAddressInformation.Address, unicastIPAddressInformation.IPv4Mask);

                        networkInfo.FootPrint = SystemInfo.GenerateHardwareId().Data;
                        networkInfo.IPAddress = ipAddress.ToString();
                        networkInfo.Type = defaultInterface.Description;
                        networkInfo.Status = defaultInterface.OperationalStatus.ToString();
                        networkInfo.SubnetMask = subnetMask.ToString();
                        networkInfo.DefaultGateway = defaultGateway.ToString();
                        networkInfo.DNServers = string.Empty;
                        networkInfo.MACAddress = defaultInterface.GetPhysicalAddress().ToString();
                    }
                }

                if (hubConnection != null && hubConnection.State == HubConnectionState.Connected)
                {
                    await hubConnection.SendAsync("SendNetworkInfo", networkInfo);
                    await Task.Delay(10000); // Délai d'exemple de 10 secondes
                }
                else
                {
                    Console.WriteLine("La connexion n'est pas établie. Arrêt de l'envoi de messages.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'envoi des informations réseau : {ex.Message}");
            }
        }

        public void CaptureDesktopImage()
        {
            isCapturing = true;
            captureThread = new Thread(CaptureScreenContinuously);
            captureThread.Start();
        }

        private void CaptureScreenContinuously()
        {
            while (isCapturing)
            {
                double screenWidth = SystemParameters.PrimaryScreenWidth;
                double screenHeight = SystemParameters.PrimaryScreenHeight;

                Bitmap screenshot = new Bitmap((int)screenWidth, (int)screenHeight);

                using (Graphics g = Graphics.FromImage(screenshot))
                {
                    g.CopyFromScreen(new System.Drawing.Point(0, 0), System.Drawing.Point.Empty, new System.Drawing.Size((int)screenWidth, (int)screenHeight));
                }

                Application.Current.Dispatcher.Invoke(() =>
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
                    file.CarteMere = SystemInfo.GetMotherboardInfo();

                    if (hubConnection != null && hubConnection.State == HubConnectionState.Connected)
                    {
                        hubConnection.SendAsync("SendImageMessage", file);
                    }
                    else
                    {
                        Console.WriteLine("La connexion n'est pas établie. Arrêt de l'envoi de messages.");
                    }
                });

                Thread.Sleep(1);
            }
        }

        private byte[] ImageToByteArray(Bitmap image)
        {
            MemoryStream stream = new MemoryStream();
            image.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
            return stream.ToArray();
        }

        public async Task StartAsync()
        {
            try
            {
                await hubConnection.StartAsync();
                Computer computer = new Computer
                {
                    CarteMere = SystemInfo.GetMotherboardInfo(),
                    GPU = SystemInfo.GetGPUName(),
                    Os = SystemInfo.GetOperatingSystemInfo(),
                    Name = SystemInfo.GetComputerName(),
                    IDProduct = SystemInfo.GetWindowsProductId(),
                    FootPrint = SystemInfo.GenerateHardwareId().Data,
                    ParcId = parc,
                    RoomId = room,
                    Statut = true,
                    IsActive = true,
                    OperatingSystem = SystemInfo.GetSystemArchitecture()
                };
                await hubConnection.SendAsync("SendInfoOfPc", computer);
                await SendNetworkInfo();
                CaptureDesktopImage();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Échec de la connexion au serveur : {ex.Message}");
            }
        }
    }

}
