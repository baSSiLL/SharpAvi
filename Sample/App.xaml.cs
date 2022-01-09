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
            var dllName = string.Format("lameenc{0}.dll", Environment.Is64BitProcess ? "64" : "32");
            Mp3LameAudioEncoder.SetLameDllLocation(Path.Combine(asmDir, dllName));
        }
    }
}
