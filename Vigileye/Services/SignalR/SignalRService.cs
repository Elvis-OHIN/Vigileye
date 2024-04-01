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
using Vigileye.Models;
using Vigileye.Services.RemoteControl;

namespace Vigileye.Services.SignalR
{
    public class SignalRService
    {
        private readonly HubConnection hubConnection;
        private InputSimulationService inputSimulationService;
        private bool isCapturing = false;
        private Thread captureThread;

        public SignalRService()
        {
            string url = ((App)Application.Current).Url;

            hubConnection = new HubConnectionBuilder()
                .WithUrl($"{url}/sharehub")
                .WithAutomaticReconnect()
                .Build();

            inputSimulationService = new InputSimulationService();

            ConfigureEventHandlers();
            
        }

        private void ConfigureEventHandlers()
        {
            hubConnection.On<string>("ReceiveKeyPress", key =>
            {
                inputSimulationService.SimulateKeyPress(key);
            });

            hubConnection.On<string>("ReceiveClickPress", clic =>
            {
                inputSimulationService.SimulateClickPress(clic);
            });

            hubConnection.On<double, double, double , double>("ReceiveMouseMovement", (x,y,height,width) =>
            {
                inputSimulationService.SimulateMouseMovement(x,y,height,width);
            });

            hubConnection.On<string, string>("ReceiveMessage", (title, message) =>
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
            });
        }

        public async Task SendNetworkInfo()
        {
            NetworkInfo networkInfo = new NetworkInfo();
            
           

            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            NetworkInterface defaultInterface = null;
            foreach (var networkInterface in networkInterfaces)
            {
                IPInterfaceProperties properties = networkInterface.GetIPProperties();
                // Vérifie si l'interface a des passerelles
                if (properties.GatewayAddresses.Count > 0)
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
                networkInfo.Id = 0;
                networkInfo.CarteMere = SystemInfo.GetMotherboardInfo();
                networkInfo.Type  = defaultInterface.Description;
                networkInfo.Status = defaultInterface.OperationalStatus.ToString();
                networkInfo.IPAddress = ipAddress.ToString();
                networkInfo.SubnetMask = subnetMask.ToString();
                networkInfo.DefaultGateway = defaultGateway.ToString();
                networkInfo.DNServers = string.Join(", ", dnsServers);
                networkInfo.MACAddress = SystemInfo.ObtenirAdresseMac();

            }
            else
            {
                System.Windows.MessageBox.Show("Aucune interface réseau active trouvée.");
            }


            if (hubConnection != null && hubConnection.State == HubConnectionState.Connected)
            {
                await hubConnection.SendAsync("SendNetworkInfo", networkInfo);

                await Task.Delay(10000); // Délai d'exemple de 1 seconde

            }
            else
            {
                Console.WriteLine("La connexion n'est pas établie. Arrêt de l'envoi de messages.");
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



        public async Task StartAsync()
        {
            try
            {
                await hubConnection.StartAsync();
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
