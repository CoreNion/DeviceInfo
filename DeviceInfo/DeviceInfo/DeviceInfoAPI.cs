using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceInfo
{
    public class DeviceInfoAPI
    {
        public interface IDeviceInfo
        {
            string ScreenHeightSize();
            string ScreenWidthSize();
            string OSVersion();
            string ModelName();
            string ModelNumber();
            string CPUName();
            string GPUName();
            string BoardName();
            string TotalMemory();
            string Cores();
            string ChipID();
            string KernelVersion();
            string CPUArch();
        }
    }
}
