using System;
using System.Runtime.InteropServices;
using Foundation;
using Metal;
using static DeviceInfo.DeviceInfoAPI;
using CoreFoundation;
using ObjCRuntime;
using AppKit;

[assembly: Xamarin.Forms.Dependency(typeof(DeviceInfo.Mac.GetDeviceInfo))]
namespace DeviceInfo.Mac
{
    public class GetDeviceInfo : IDeviceInfo
    {
        IMTLDevice mtlDevice;
        NSProcessInfo processInfo = new NSProcessInfo();

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

            if (isInt)
            {
                ret = Marshal.ReadInt32(oldp).ToString();
            }
            else
            {
                ret = Marshal.PtrToStringAnsi(oldp);
            }

            Marshal.FreeHGlobal(oldlenp);
            Marshal.FreeHGlobal(oldp);

            return ret;
        }

        //Using IOKit
        [DllImport("/System/Library/Frameworks/IOKit.framework/IOKit")]
        static extern /* CFMutableDictionaryRef */ IntPtr IOServiceMatching(string name);
        [DllImport("/System/Library/Frameworks/IOKit.framework/IOKit")]
        static extern /* io_service_t */ IntPtr IOServiceGetMatchingService(/* mach_port_t */ IntPtr masterPort, /* CFDictionaryRef */ IntPtr matching);
        [DllImport("/System/Library/Frameworks/IOKit.framework/IOKit")]
        static extern /* CFTypeRef */ IntPtr IORegistryEntryCreateCFProperty(/* io_registry_entry_t */ IntPtr entry, /* CFStringRef */ IntPtr key, /* CFAllocatorRef */IntPtr allocator, /* IOOptionBits */ UInt32 options);

        public string BoardName()
        {
            //Get from IORegistry
            IntPtr IOPlatformExpertDevice = IOServiceGetMatchingService(IntPtr.Zero, IOServiceMatching("IOPlatformExpertDevice"));

            // ARM Macs and Intel Macs have different property that should be obtained.
            int cpusubtype = int.Parse(SysctlAnswer("hw.cpusubtype", true));
            CFString cF;
            if (int.Parse(SysctlAnswer("sysctl.proc_translated", true)) == 1 || cpusubtype == 2 || cpusubtype == 1 || cpusubtype == 0)
                cF = new CFString("compatible");
            else 
                cF = new CFString("board-id");

            IntPtr model = IORegistryEntryCreateCFProperty(IOPlatformExpertDevice, cF.Handle, IntPtr.Zero, 0);
            return Runtime.GetNSObject<NSObject>(model).ToString();

            // Example
            // M1 MacBook Air(MacBookAir10,1) = J313AP
            // MacBook Pro 2018(MacBookPro15,2) = Mac-827FB448E656EC26
        }

        public string ChipID()
        {
            // The method of obtaining the CPU (Chip) ID is different for ARM Macs and Intel Macs.
            int cpusubtype = int.Parse(SysctlAnswer("hw.cpusubtype", true));
            if (int.Parse(SysctlAnswer("sysctl.proc_translated", true)) == 1 || cpusubtype == 2 || cpusubtype == 1 || cpusubtype == 0)
            {
                //Get from IORegistry
                IntPtr IOPlatformExpertDevice = IOServiceGetMatchingService(IntPtr.Zero, IOServiceMatching("IOPlatformExpertDevice"));
                var cF = new CFString("platform-name");
                IntPtr platform_name = IORegistryEntryCreateCFProperty(IOPlatformExpertDevice, cF.Handle, IntPtr.Zero, 0);
                return Runtime.GetNSObject<NSObject>(platform_name).ToString();
            } else {
                string Family = SysctlAnswer("machdep.cpu.family", true);
                string Model = SysctlAnswer("machdep.cpu.model", true);
                return "CPUFamily:" + Family + " Model:" + Model;
            }
        }

        public string Cores() => SysctlAnswer("hw.physicalcpu_max", true);

        public string CPUArch()
        {
            int cpusubtype = int.Parse(SysctlAnswer("hw.cpusubtype", true));
            string ret;

            // https://opensource.apple.com/source/xnu/xnu-7195.81.3/osfmk/mach/machine.h.auto.html
            // 64Bit only because this app only compile 64bit app.
            if (cpusubtype == 1)
                ret = "ARMv8 (ARM64)";
            else if (cpusubtype == 2)
                ret = "ARM64e";
            else if (cpusubtype == 0)
                ret = "ARM64";
            else if (cpusubtype == 8 || cpusubtype == 4 || cpusubtype == 3)
                ret = "x86_64";
            else
                ret = "Unknown (CPU subtype:" + cpusubtype + ")";
            if (int.Parse(SysctlAnswer("sysctl.proc_translated", true)) == 1)
                ret += "　(Translated by Rosetta)";
            return ret;
        }

        public string CPUName() => SysctlAnswer("machdep.cpu.brand_string", false);

        public string GPUName()
        {
            mtlDevice = MTLDevice.SystemDefault;
            return mtlDevice.Name;
        }

        public string KernelVersion() => SysctlAnswer("kern.version", false);

        public string ModelName()
        {
            //Get Form com.apple.SystemProfiler.plist
            string PlistPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/Library/Preferences/com.apple.SystemProfiler.plist";
            NSDictionary plist = NSDictionary.FromFile(PlistPath);
            var hasKey = plist.ContainsKey((NSString)"CPU Names");
            if (hasKey == true)
            {
                NSDictionary CPUKey = (NSDictionary)plist.ValueForKey((NSString)"CPU Names");
                NSString DeviceModel = (NSString)CPUKey.Values[0];
                return DeviceModel.ToString();
            }
            else
            {
                return "Unknown";
            }
        }

        public string ModelNumber() => SysctlAnswer("hw.model",false);


        public string OSVersion()
        {
            string OSName;
            var version = NSProcessInfo.ProcessInfo.OperatingSystemVersion;
            int MainVer = (int)version.Major;
            int SubVer = (int)version.Minor;
            int PatchVer = (int)version.PatchVersion;
            if (SubVer > 11 || MainVer == 11)
                OSName = "macOS";
            else
                OSName = "OSX";

            return OSName + " " + MainVer.ToString() + "." + SubVer.ToString() + "." + PatchVer.ToString() + " (" + SysctlAnswer("kern.osversion",false) + ")";
        }

        NSDictionary screenDescription = NSScreen.MainScreen.DeviceDescription;

        public string ScreenHeightSize()
        {
            var displayPixelSize = (screenDescription.ObjectForKey(new NSString("NSDeviceSize")) as NSValue).CGSizeValue;
            return displayPixelSize.Height.ToString();
        }

        public string ScreenWidthSize()
        {
            var displayPixelSize = (screenDescription.ObjectForKey(new NSString("NSDeviceSize")) as NSValue).CGSizeValue;
            return displayPixelSize.Width.ToString();
        }

        public string TotalMemory() => (processInfo.PhysicalMemory / 1024 / 1024).ToString() + "MB";
    }
}
