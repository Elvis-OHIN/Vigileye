using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsInput.Native;
using WindowsInput;
using System.Windows.Forms;

namespace Vigileye.Services.RemoteControl
{
    public class InputSimulationService
    {
        private readonly InputSimulator inputSimulator = new InputSimulator();
        private KeyMappingService keyMappingService;

        public InputSimulationService()
        {
            keyMappingService = new KeyMappingService();
        }

        // Simule l'appui d'une touche du clavier
        public void SimulateKeyPress(string key)
        {
            try
            {
                VirtualKeyCode keyCode = keyMappingService.ConvertToVirtualKeyCode(key);
                inputSimulator.Keyboard.KeyPress(keyCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la simulation de l'appui sur la touche {key} : {ex.Message}");
            }
        }

        // Simule un clic de souris
        public void SimulateClickPress(string clic)
        {
            try
            {
                if (clic == "Gauche")
                {
                    inputSimulator.Mouse.LeftButtonClick();
                }
                else if (clic == "Droit")
                {
                    inputSimulator.Mouse.RightButtonClick();
                }
                else if (clic == "Milieu")
                {
                    inputSimulator.Mouse.MiddleButtonClick();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la simulation du clic {clic} : {ex.Message}");
            }
        }

        // Simule le mouvement de la souris
        public void SimulateMouseMovement(double x, double y, double height, double width)
        {
            try
            {
                var largeurServeur = Screen.PrimaryScreen.Bounds.Width;
                var hauteurServeur = Screen.PrimaryScreen.Bounds.Height;

                // Calculer les facteurs d'échelle
                double facteurEchelleX = (double)largeurServeur / width;
                double facteurEchelleY = (double)hauteurServeur / height;

                // Adapter les coordonnées
                int xServeur = (int)(x * facteurEchelleX);
                int yServeur = (int)(y * facteurEchelleY);

                var xVirtual = ConvertToVirtualDesktopCoordinate(xServeur, largeurServeur);
                var yVirtual = ConvertToVirtualDesktopCoordinate(yServeur, hauteurServeur);

                inputSimulator.Mouse.MoveMouseToPositionOnVirtualDesktop(xVirtual, yVirtual);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la simulation du mouvement de la souris : {ex.Message}");
            }
        }

        // Convertit une coordonnée en pixel en une coordonnée sur le bureau virtuel de Windows
        private static double ConvertToVirtualDesktopCoordinate(int pixelCoordinate, int screenSize)
        {
            try
            {
                return (65535.0 * pixelCoordinate) / screenSize;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la conversion des coordonnées : {ex.Message}");
                return 0;
            }
        }
    }

}
