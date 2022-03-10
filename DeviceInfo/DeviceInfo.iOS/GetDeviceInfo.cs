using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UIKit;
using Xamarin.iOS;
using XamDeviceInfo = Xamarin.Essentials.DeviceInfo;
using static DeviceInfo.DeviceInfoAPI;
using CoreFoundation;
using ObjCRuntime;
using Metal;

[assembly: Xamarin.Forms.Dependency(typeof(DeviceInfo.iOS.GetDeviceInfo))]
namespace DeviceInfo.iOS
{
    class GetDeviceInfo : IDeviceInfo
    {
        [DllImport(ObjCRuntime.Constants.SystemLibrary)]
        static extern int sysctlbyname([MarshalAs(UnmanagedType.LPStr)] string name, IntPtr oldp, IntPtr oldlenp, IntPtr newp, uint newlen);
        public static string SysctlAnswer(string name, bool isInt)
        {
            var oldlenp = Marshal.AllocHGlobal(sizeof(int));
            sysctlbyname(name, IntPtr.Zero, oldlenp, IntPtr.Zero, 0);

            var length = Marshal.ReadInt32(oldlenp);

            var oldp = Marshal.AllocHGlobal(length);
            sysctlbyname(name, oldp, oldlenp, IntPtr.Zero, 0);

            string ret;

            if(isInt) {
                ret = Marshal.ReadInt32(oldp).ToString();
            } else
            {
                ret = Marshal.PtrToStringAnsi(oldp);
            }

            Marshal.FreeHGlobal(oldlenp);
            Marshal.FreeHGlobal(oldp);

            return ret;
        }

        public string BoardName() => SysctlAnswer("hw.model", false);

        [DllImport("/usr/lib/libMobileGestalt.dylib")]
        extern static /* CFStringRef */ IntPtr MGCopyAnswer(IntPtr property /* CFStringRef  property*/ );

        public string ChipID()
        {
            var CF = new CFString("HardwarePlatform");
            var Answer = MGCopyAnswer(CF.Handle);
            return Runtime.GetNSObject<NSObject>(Answer).ToString();
        }

        public string Cores() => SysctlAnswer("hw.physicalcpu_max", true);

        public string CPUName()
        {
            string chipid = ChipID();
            string board = ModelNumber();

            if (chipid == "s5l8960x" || chipid == "s5l8965x")
                return "A7";
            else if (chipid == "t7000")
                return "A8";
            else if (chipid == "t7001")
                return "A8X";
            else if (chipid == "s8000" || chipid == "s8003")
                return "A9";
            else if (chipid == "s8001")
                return "A9X";
            else if (chipid == "t8010")
                return "A10 Fusion";
            else if (chipid == "t8011")
                return "A10X Fusion";
            else if (chipid == "t8015")
                return "A11 Bionic";
            else if (chipid == "t8020")
                return "A12 Bionic";
            else if (chipid == "t8027")
            {
                // A12X = A12Z
                if (board == "J420AP" || board == "J421AP")
                    return "A12Z Bionic";
                else return "A12X Bionic";
            }
            else if (chipid == "t8030")
                return "A13 Bionic";
            else if (chipid == "t8101")
                return "A14 Bionic";
            else if (chipid == "t8110")
                return "A15 Bionic";
            else if (chipid == "t8103")
                return "M1";
            else if (chipid == "t6000")
                return "M1 Pro";
            else if (chipid == "t6001")
                return "M1 Max";
            else if (chipid == "t6002")
                return "M1 Ultra";
            else return "Unknown";
        }

        IMTLDevice mtlDevice;
        public string GPUName()
        {
            //Get From Metal
            mtlDevice = MTLDevice.SystemDefault;
            return mtlDevice.Name;
        }

        public string KernelVersion() => SysctlAnswer("kern.version", false);

        public string ModelName() => DeviceHardware.Model;

        //Using  Xamarin.Essentials.DeviceInfo
        public string ModelNumber() => XamDeviceInfo.Model;

        UIDevice uIDevice = new UIDevice();
        public string OSVersion() => String.Format("{0} {1} ({2})", uIDevice.SystemName, uIDevice.SystemVersion, SysctlAnswer("kern.osversion", false));

        public string ScreenHeightSize() => UIScreen.MainScreen.NativeBounds.Height.ToString();
        public string ScreenWidthSize() => UIScreen.MainScreen.NativeBounds.Width.ToString();

        NSProcessInfo processInfo = new NSProcessInfo();
        public string TotalMemory() => (processInfo.PhysicalMemory / 1024 / 1024).ToString() + "MB";

        public string CPUArch() {
            int cpusubtype = int.Parse(SysctlAnswer("hw.cpusubtype", true));

            // https://opensource.apple.com/source/xnu/xnu-7195.81.3/osfmk/mach/machine.h.auto.html
            // 64Bit only because this app only compile 64bit app.
            if (cpusubtype == 1)
                return "ARMv8 (ARM64)";
            else if (cpusubtype == 2)
                return "ARM64e";
            else if (cpusubtype == 0)
                return "ARM64";
            else if (cpusubtype == 8 || cpusubtype == 3)
                return "x86_64";
            else
                return "Unknown (CPU subtype:" + cpusubtype + ")";
        }
    }
}