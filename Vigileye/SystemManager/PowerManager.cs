
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Principal;
namespace Vigileye.SystemManager
{
    public static class PowerManager
    {
        // Importation des fonctions externes pour gérer les états de puissance du système
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ExitWindowsEx(uint uFlags, uint dwReason);

        [DllImport("powrprof.dll", SetLastError = true)]
        private static extern bool SetSuspendState(bool hibernate, bool forceCritical, bool disableWakeEvent);

        // Constantes pour les drapeaux des options d'alimentation
        private const uint EWX_LOGOFF = 0x00000000;
        private const uint EWX_SHUTDOWN = 0x00000001;
        private const uint EWX_REBOOT = 0x00000002;
        private const uint EWX_FORCE = 0x00000004;
        private const uint EWX_POWEROFF = 0x00000008;
        private const uint EWX_FORCEIFHUNG = 0x00000010;

        // Méthode pour éteindre le système
        public static void Shutdown(bool force = false)
        {
            try
            {
                Process.Start("shutdown", "/s /t 0");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'arrêt du système : {ex.Message}");
            }
        }

        // Méthode pour redémarrer le système
        public static void Reboot(bool force = false)
        {
            try
            {
                uint flags = EWX_REBOOT;
                if (force)
                {
                    flags |= EWX_FORCE;
                }
                if (!ExitWindowsEx(flags, 0))
                {
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du redémarrage du système : {ex.Message}");
            }
        }

        // Méthode pour mettre le système en veille ou en hibernation
        public static void Sleep(bool hibernate = false, bool force = false, bool disableWakeEvent = false)
        {
            try
            {
                if (!SetSuspendState(hibernate, force, disableWakeEvent))
                {
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la mise en veille ou en hibernation du système : {ex.Message}");
            }
        }
    }
}



