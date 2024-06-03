using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.IO;
using System.Diagnostics;
using Microsoft.VisualBasic;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using SynetraUtils.Auth;
using Org.BouncyCastle.Security;
using SynetraUtils.Models.MessageManagement;
using System.Net;

namespace Vigileye.SystemManager
{
    public class SystemInfo
    {
        // Récupère le nom de l'ordinateur
        public static string GetComputerName()
        {
            return Environment.MachineName;
        }

        // Récupère l'architecture du système (64 bits ou 32 bits)
        public static string GetSystemArchitecture()
        {
            return Environment.Is64BitOperatingSystem ? "64 bits" : "32 bits";
        }

        // Récupère les informations sur le système d'exploitation
        public static string GetOperatingSystemInfo()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT Caption, Version FROM Win32_OperatingSystem"))
                {
                    foreach (var os in searcher.Get())
                    {
                        return $"{os["Caption"]} - Version {os["Version"]}";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération des informations sur le système d'exploitation : {ex.Message}");
            }
            return "Non trouvé";
        }

        // Récupère le nom de la carte graphique
        public static string GetGPUName()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        return obj["Name"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération du nom de la carte graphique : {ex.Message}");
            }
            return "Non trouvé";
        }

        // Récupère les informations sur la carte mère
        public static string GetMotherboardInfo()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        return $"Fabricant: {obj["Manufacturer"]}, Produit: {obj["Product"]}";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération des informations sur la carte mère : {ex.Message}");
            }
            return "Non trouvé";
        }

        // Récupère l'ID du produit Windows
        public static string GetWindowsProductId()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        return obj["SerialNumber"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération de l'ID du produit Windows : {ex.Message}");
            }
            return "Non trouvé";
        }

        // Récupère l'adresse de diffusion en fonction de l'adresse IP et du masque de sous-réseau
        public static IPAddress GetBroadcastAddress(IPAddress address, IPAddress subnetMask)
        {
            try
            {
                byte[] ipAdressBytes = address.GetAddressBytes();
                byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

                if (ipAdressBytes.Length != subnetMaskBytes.Length)
                    throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

                byte[] broadcastAddress = new byte[ipAdressBytes.Length];
                for (int i = 0; i < broadcastAddress.Length; i++)
                {
                    broadcastAddress[i] = (byte)(ipAdressBytes[i] | subnetMaskBytes[i] ^ 255);
                }
                return new IPAddress(broadcastAddress);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du calcul de l'adresse de diffusion : {ex.Message}");
                return null;
            }
        }

        // Récupère l'interface réseau principale
        public static NetworkInterface GetPrimaryNetworkInterface()
        {
            try
            {
                return NetworkInterface.GetAllNetworkInterfaces()
                    .FirstOrDefault(nic => nic.OperationalStatus == OperationalStatus.Up &&
                                           (nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet || nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération de l'interface réseau principale : {ex.Message}");
                return null;
            }
        }

        // Récupère la mémoire totale en octets
        public static ulong GetTotalMemoryInBytes()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("select * from Win32_ComputerSystem"))
                {
                    foreach (var item in searcher.Get())
                    {
                        return (ulong)item["TotalPhysicalMemory"];
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération de la mémoire totale : {ex.Message}");
            }
            return 0;
        }

        // Génère un ID matériel chiffré
        public static EncryptedData GenerateHardwareId()
        {
            try
            {
                var processorId = GetWmiDeviceProperty("Win32_Processor", "ProcessorId");
                var diskDriveId = GetWmiDeviceProperty("Win32_DiskDrive", "SerialNumber");
                var motherboardId = GetWmiDeviceProperty("Win32_BaseBoard", "SerialNumber");

                var combinedId = $"{processorId}:{diskDriveId}:{motherboardId}";

                EncryptedData encryptedData = new EncryptedData();

                using (Aes myAes = Aes.Create())
                {
                    // Encrypt the string to an array of bytes.
                    byte[] encrypted = HardwareIdentifier.EncryptStringToBytes_Aes(combinedId, myAes.Key, myAes.IV);
                    encryptedData.Data = combinedId;
                    encryptedData.Key = Convert.ToBase64String(myAes.Key);
                    encryptedData.IV = Convert.ToBase64String(myAes.IV);
                }

                return encryptedData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la génération de l'ID matériel : {ex.Message}");
                return null;
            }
        }

        // Récupère une propriété de périphérique via WMI
        private static string GetWmiDeviceProperty(string wmiClass, string property)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher($"SELECT {property} FROM {wmiClass}"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        return obj[property]?.ToString().Trim() ?? string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                // Gérer les exceptions ou les enregistrer
                Console.WriteLine($"Une erreur s'est produite lors de la requête WMI : {ex.Message}");
            }
            return string.Empty;
        }
    }
}
