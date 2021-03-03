using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Opengl;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Lang;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static DeviceInfo.DeviceInfoAPI;

[assembly: Xamarin.Forms.Dependency(typeof(DeviceInfo.Droid.GetDeviceInfo))]
namespace DeviceInfo.Droid
{
    class GetDeviceInfo : IDeviceInfo
    {
        public string BoardName() => Build.Board;

        private string[] CPUInfoSercher()
        {
            // Getting CPU Infomation from /proc/cpuinfo
            // ["CPU Name","Vendor id/name", "ChipID"]
            string[] ret = new string[] { "Unknown", "Unknown", "Unknown" };

            string fileName = @"/proc/cpuinfo";
            IEnumerable<string> lines = File.ReadLines(fileName);
            foreach (string line in lines)
            {
                Console.WriteLine(line);
                if (0 <= line.IndexOf("Hardware") || 0 <= line.IndexOf("model name") || 0 <= line.IndexOf("Processor"))
                {
                    ret[0] = line.Substring(line.IndexOf(":") + 1);
                }
                if (0 <= line.IndexOf("CPU implementer") || 0 <= line.IndexOf("vendor_id"))
                {
                    ret[1] = line.Substring(line.IndexOf(":") + 1);
                }
                if (0 <= line.IndexOf("CPU part") || (0 <= line.IndexOf("model") && !(0 <= line.IndexOf("model name"))))
                {
                    ret[2] = line.Substring(line.IndexOf(":") + 1);
                }
            }
            return ret;
        }

        public string ChipID() => "Vendor(implementer) id:" + CPUInfoSercher()[1] + System.Environment.NewLine + "CPU ID:" + CPUInfoSercher()[2];

        public string Cores() => Runtime.GetRuntime().AvailableProcessors().ToString();

        public string CPUArch() => Build.CpuAbi;

        public string CPUName() => CPUInfoSercher()[0];

        public string GPUName()
        {
            //Get GPU Name using OpenGL.
            //To do glGetString, it need to initialize OpenGL.
            int[] iparam = new int[8];

            int[] major = new int[1];
            int[] minor = new int[1];
            EGLDisplay gLDisplay = EGL14.EglGetDisplay(EGL14.EglDefaultDisplay);
            EGL14.EglInitialize(gLDisplay, major, 0, minor, 0);

            int[] config = { EGL14.EglRenderableType, EGL14.EglOpenglEs2Bit, EGL14.EglNone };
            int config_size = 1;
            EGLConfig[] gLConfigs = new EGLConfig[config_size];
            EGL14.EglChooseConfig(gLDisplay, config, 0, gLConfigs, 0, config_size, iparam, 0);
            EGLConfig gLConfig = gLConfigs[0];

            SurfaceTexture surfaceTexture = new SurfaceTexture(0);
            EGLSurface gLSurface = EGL14.EglCreateWindowSurface(gLDisplay, gLConfig, surfaceTexture, config, 0);

            int[] contexts = { EGL14.EglContextClientVersion, 2, EGL14.EglNone };
            EGLContext gLContext = EGL14.EglCreateContext(gLDisplay, gLConfig, EGL14.EglNoContext, contexts, 0);

            EGL14.EglMakeCurrent(gLDisplay, gLSurface, gLSurface, gLContext);

            return GLES20.GlGetString(GLES20.GlRenderer).ToString();
        }

        public string KernelVersion() => JavaSystem.GetProperty("os.name") + " " + JavaSystem.GetProperty("os.version");

        public string ModelName() => string.Format("{0} {1}", Build.Manufacturer, Build.Model);

        public string ModelNumber() => Build.Device;

        public string OSVersion() => string.Format("Android {0} (API {1})" + System.Environment.NewLine + "{2}", Build.VERSION.Release, Build.VERSION.SdkInt, Build.Display);

        //Get Resolution using WindowManager
        readonly IWindowManager windowManager = Application.Context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
        readonly Point prealsize = new Point();
        public string ScreenHeightSize()
        {
            Point prealsize = new Point();
            windowManager.DefaultDisplay.GetRealSize(prealsize);
            return prealsize.X.ToString();
        }

        public string ScreenWidthSize()
        {
            windowManager.DefaultDisplay.GetRealSize(prealsize);
            return prealsize.Y.ToString();
        }

        public string TotalMemory()
        {
            ActivityManager actManager = (ActivityManager)Application.Context.GetSystemService(Context.ActivityService);
            ActivityManager.MemoryInfo memInfo = new ActivityManager.MemoryInfo();
            actManager.GetMemoryInfo(memInfo);
            return (memInfo.TotalMem / 1024 / 1024).ToString() + "MB";
        }
    }
}