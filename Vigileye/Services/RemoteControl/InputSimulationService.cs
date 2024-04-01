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

        public void SimulateKeyPress(string key)
        {
            VirtualKeyCode keyCode = keyMappingService.ConvertToVirtualKeyCode(key);
            inputSimulator.Keyboard.KeyPress(keyCode);
        }

        public void SimulateClickPress(string clic)
        {
            if (clic == "Gauche")
            {
                inputSimulator.Mouse.LeftButtonClick();
            }
            else if (clic  == "Droit")
            {
                inputSimulator.Mouse.RightButtonClick();
            }
            else if (clic == "Milieu")
            {
                inputSimulator.Mouse.MiddleButtonClick();
            }
        }

        public void SimulateMouseMovement(double x, double y , double height , double width)
        {
            var inputSimulator = new InputSimulator();
            var largeurServeur = Screen.PrimaryScreen.Bounds.Width;
            var hauteurServeur = Screen.PrimaryScreen.Bounds.Height;

            // Calculer les facteurs d'échelle
            double facteurEchelleX = (double)largeurServeur / width;
            double facteurEchelleY = (double)hauteurServeur / height;

            // Adapter les coordonnées
            int xServeur = (int)(x * facteurEchelleX);
            int yServeur = (int)(y * facteurEchelleY);

            var xVirtual = ConvertToVirtualDesktopCoordinate((int)x, 1200);
            var yVirtual = ConvertToVirtualDesktopCoordinate((int)y, 350);

            inputSimulator.Mouse.MoveMouseToPositionOnVirtualDesktop(xVirtual, yVirtual);
        }
        private static double ConvertToVirtualDesktopCoordinate(int pixelCoordinate, int screenSize)
        {
            // Le bureau virtuel de Windows a une échelle de 0 à 65535, indépendamment de la résolution de l'écran
            return (65535.0 * pixelCoordinate) / screenSize;
        }
    }
}
