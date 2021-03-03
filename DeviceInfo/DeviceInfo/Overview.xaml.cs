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
    public partial class Overview : ContentPage
    {
        public Overview()
        {
            InitializeComponent();
            DeviceInfoAPI.IDeviceInfo deviceInfo = DependencyService.Get<DeviceInfoAPI.IDeviceInfo>();
            //Tasking because it's heavy.
            var tasks = new List<Task>();

            tasks.Add(Task.Run(() => ModelName.Text = deviceInfo.ModelName()));
            tasks.Add(Task.Run(() => ModelNumber.Text = deviceInfo.ModelNumber()));
            tasks.Add(Task.Run(() => OSVersion.Text = deviceInfo.OSVersion()));
            tasks.Add(Task.Run(() => ScreenWSize.Text = deviceInfo.ScreenWidthSize()));
            tasks.Add(Task.Run(() => ScreenHSize.Text = deviceInfo.ScreenHeightSize()));
            tasks.Add(Task.Run(() => BoardName.Text = deviceInfo.BoardName()));
            tasks.Add(Task.Run(() => KernVersion.Text = deviceInfo.KernelVersion()));
            Task.WhenAll(tasks);
        }
    }
}