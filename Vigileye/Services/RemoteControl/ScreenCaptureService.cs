using SynetraUtils.Models.MessageManagement;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Vigileye.Services.RemoteControl
{
    public class ScreenCaptureService
    {
        private static bool isCapturing = false;
        private static Thread captureThread;

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
                    return file;
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
    }
}
