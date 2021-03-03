using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using static DeviceInfo.DeviceInfoAPI;
using System.Diagnostics;
using Microsoft.Win32;
using System.Windows;

[assembly: Xamarin.Forms.Dependency(typeof(DeviceInfo.WPF.GetDeviceInfo))]
namespace DeviceInfo.WPF
{
    class GetDeviceInfo : IDeviceInfo
    {
        private readonly ManagementObject Win32_BaseBoard = new ManagementObjectSearcher("Select * From Win32_BaseBoard").Get().Cast<ManagementObject>().First();
        public string BoardName()
        {
            string motherMaker = Win32_BaseBoard["Manufacturer"].ToString();
            string motherName = Win32_BaseBoard["Product"].ToString();
            return motherMaker + " " + motherName;
        }

        private readonly ManagementObject Win32_Processor = new ManagementObjectSearcher("select * from Win32_Processor").Get().Cast<ManagementObject>().First();
        public string ChipID() => Win32_Processor["Caption"].ToString();

        public string Cores() => Win32_Processor["NumberOfCores"].ToString();

        public string CPUArch() =>Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE", EnvironmentVariableTarget.Machine);

        public string CPUName() => Win32_Processor["Name"].ToString();

        private readonly ManagementObject Win32_VideoController = new ManagementObjectSearcher("select * from Win32_VideoController").Get().Cast<ManagementObject>().First();
        public string GPUName() => Win32_VideoController["Name"].ToString();

        //Get from ntoskrnl.exe
        public string KernelVersion()
        {
            try
            {
                return FileVersionInfo.GetVersionInfo("C:\\Windows\\System32\\ntoskrnl.exe").FileVersion;
            } catch
            {
                return "Unknown";
            }
        }

        private readonly ManagementObject Win32_ComputerSystemProduct = new ManagementObjectSearcher("select * from Win32_ComputerSystemProduct").Get().Cast<ManagementObject>().First();
        public string ModelName()
        {
            string Number = Win32_ComputerSystemProduct["Name"].ToString();
            if (Number == "To Be Filled By O.E.M.")
                return "Empty (To Be Filled By O.E.M.)";
            return Win32_ComputerSystemProduct["Name"].ToString();
        }

        public string ModelNumber()
        {
            string Number = Win32_ComputerSystemProduct["IdentifyingNumber"].ToString();
            if (Number == "To Be Filled By O.E.M.")
                return "Empty (To Be Filled By O.E.M.)";
            return Win32_ComputerSystemProduct["IdentifyingNumber"].ToString();
        }

        private readonly ManagementObject Win32_OperatingSystem = new ManagementObjectSearcher("select * from Win32_OperatingSystem").Get().Cast<ManagementObject>().First();
        public string OSVersion()
        {
            string OSName = Win32_OperatingSystem["Caption"].ToString();
            string Build = Win32_OperatingSystem["BuildNumber"].ToString() + "." + Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion").GetValue("UBR").ToString();
            string ReleaseVer = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "DisplayVersion", "").ToString();
            return OSName + Environment.NewLine  + ReleaseVer + " (Build " + Build + ")";
        }

        public string ScreenHeightSize() => System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height.ToString();

        public string ScreenWidthSize() => System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width.ToString();
        public string TotalMemory() => ((UInt64)Win32_OperatingSystem["TotalVisibleMemorySize"] / 1024).ToString() + "MB";
    }
}
