using System;
using System.IO;
using System.Reflection;
using System.Windows;
using SharpAvi.Codecs;

namespace SharpAvi.Sample
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Set LAME DLL path for MP3 encoder
            var asmDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
#if FX45
            var is64BitProcess = Environment.Is64BitProcess;
#else
            var is64BitProcess = IntPtr.Size * 8 == 64;
#endif
            var dllName = string.Format("lameenc{0}.dll", is64BitProcess ? "64" : "32");
            Mp3AudioEncoderLame.SetLameDllLocation(Path.Combine(asmDir, dllName));
        }
    }
}
