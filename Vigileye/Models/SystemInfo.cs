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

namespace Vigileye.Models
{
    public class SystemInfo
    {
        public static string GetComputerName()
        {
            return Environment.MachineName;
        }

        public static string GetOSVersion()
        {
            return Environment.OSVersion.ToString();
        }
        public static string GetProcessorName()
        {
            string processorName = "";
            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
            foreach (var item in searcher.Get())
            {
                processorName = item["Name"].ToString();
                break; // Généralement, il n'y a qu'un seul processeur
            }
            return processorName;
        }

        public static string GetSystemArchitecture()
        {
            return Environment.Is64BitOperatingSystem ? "64 bits" : "32 bits";
        }
        public static string GetOperatingSystemInfo()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT Caption, Version FROM Win32_OperatingSystem"))
            {
                foreach (var os in searcher.Get())
                {
                    return $"{os["Caption"]} - Version {os["Version"]}";
                }
            }
            return "Non trouvé";
        }

        public static string GetGPUName()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController"))
            {
                foreach (var obj in searcher.Get())
                {
                    return obj["Name"].ToString();
                }
            }
            return "Non trouvé";
        }

        public static string GetMotherboardInfo()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard"))
            {
                foreach (var obj in searcher.Get())
                {
                    return $"Fabricant: {obj["Manufacturer"]}, Produit: {obj["Product"]}";
                }
            }
            return "Non trouvé";
        }
        public static string GetWindowsProductId()
        {
            string productId = "";
            ManagementObjectSearcher searcher =
                new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");

            foreach (ManagementObject obj in searcher.Get())
            {
                productId = obj["SerialNumber"].ToString();
                break;
            }

            return productId;
        }
        public static List<KeyValuePair<string, KeyValuePair<long, long>>> GetStorageInfo()
        {
            var storageInfo = new List<KeyValuePair<string, KeyValuePair<long, long>>>();
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {
                    long totalSpaceGB = drive.TotalSize / (1024 * 1024 * 1024); // Convertit en GB
                    long freeSpaceGB = drive.TotalFreeSpace / (1024 * 1024 * 1024); // Convertit en GB
                    long usedSpaceGB = totalSpaceGB - freeSpaceGB;
                    storageInfo.Add(new KeyValuePair<string, KeyValuePair<long, long>>(drive.Name, new KeyValuePair<long, long>(usedSpaceGB, freeSpaceGB)));
                }
            }
            return storageInfo;
        }
        public static ulong GetTotalMemoryInBytes()
        {
            var searcher = new ManagementObjectSearcher("select * from Win32_ComputerSystem");

            foreach (var item in searcher.Get())
            {
                return (ulong)item["TotalPhysicalMemory"];
            }

            return 0;
        }

        public static (long TotalMemory, long FreeMemory) GetRAMInfo()
        {

            var totalMemoryInBytes = ((long)GetTotalMemoryInBytes());
            var freeMemoryInBytes = new PerformanceCounter("Memory", "Available Bytes").RawValue;

            long totalMemory = totalMemoryInBytes / (1024 * 1024); // Conversion en MB
            long freeMemory = freeMemoryInBytes / (1024 * 1024); // Conversion en MB

            return (totalMemory, freeMemory);
        }
        public static string ObtenirAdresseMac()
        {
            string adresseMac = string.Empty;

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.NetworkInterfaceType != NetworkInterfaceType.Loopback && nic.NetworkInterfaceType != NetworkInterfaceType.Tunnel && nic.OperationalStatus == OperationalStatus.Up)
                {
                    adresseMac = BitConverter.ToString(nic.GetPhysicalAddress().GetAddressBytes());
                    break; // Prendre la première adresse MAC disponible
                }
            }

            return adresseMac;
        }
        public static EncryptedData GenerateHardwareId()
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
                // Handle exceptions or log them
                Console.WriteLine($"An error occurred while querying WMI: {ex.Message}");
            }

            return string.Empty;
        }

        private static string GetHashString(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }
    
    }
}
