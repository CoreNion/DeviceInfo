using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DeviceInfo
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SoCInfo : ContentPage
    {
        public SoCInfo()
        {
            InitializeComponent();
            DeviceInfoAPI.IDeviceInfo deviceInfo = DependencyService.Get<DeviceInfoAPI.IDeviceInfo>();
            //Tasking because it's heavy.
            var tasks = new List<Task>();

            tasks.Add(Task.Run(() => CPUName.Text = deviceInfo.CPUName()));
            tasks.Add(Task.Run(() => CPUArch.Text = deviceInfo.CPUArch()));
            tasks.Add(Task.Run(() => CPUCores.Text = deviceInfo.Cores()));
            tasks.Add(Task.Run(() => ChipID.Text = deviceInfo.ChipID()));
            tasks.Add(Task.Run(() => TotalMemory.Text = deviceInfo.TotalMemory()));
            tasks.Add(Task.Run(() => GPUName.Text = deviceInfo.GPUName()));
            Task.WhenAll(tasks);
        }
    }
}