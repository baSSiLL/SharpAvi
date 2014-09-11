using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using NAudio.Wave;
using SharpAvi.Codecs;

namespace SharpAvi.Sample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            recordingTimer = new Timer(recordingTimer_Tick);
            DataContext = this;

            InitDefaultSettings();

            WindowMoveBehavior.Attach(this);
        }


        #region Recording

        private readonly Timer recordingTimer;
        private DateTime recordingStartTime;
        private Recorder recorder;
        private string lastFileName;

        private static readonly DependencyPropertyKey IsRecordingPropertyKey =
            DependencyProperty.RegisterReadOnly("IsRecording", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));
        public static readonly DependencyProperty IsRecordingProperty = IsRecordingPropertyKey.DependencyProperty;

        public bool IsRecording
        {
            get { return (bool)GetValue(IsRecordingProperty); }
            private set { SetValue(IsRecordingPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey ElapsedPropertyKey =
            DependencyProperty.RegisterReadOnly("Elapsed", typeof(string), typeof(MainWindow), new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty ElapsedProperty = ElapsedPropertyKey.DependencyProperty;

        public string Elapsed
        {
            get { return (string)GetValue(ElapsedProperty); }
            private set { SetValue(ElapsedPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey HasLastScreencastPropertyKey =
            DependencyProperty.RegisterReadOnly("HasLastScreencast", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));
        public static readonly DependencyProperty HasLastScreencastProperty = HasLastScreencastPropertyKey.DependencyProperty;

        public bool HasLastScreencast
        {
            get { return (bool)GetValue(HasLastScreencastProperty); }
            private set { SetValue(HasLastScreencastPropertyKey, value); }
        }

        private void StartRecording()
        {
            if (IsRecording)
                throw new InvalidOperationException("Already recording.");

            if (minimizeOnStart)
                WindowState = WindowState.Minimized;

            Elapsed = "00:00";
            HasLastScreencast = false;
            IsRecording = true;

            recordingStartTime = DateTime.Now;
            recordingTimer.Change(1000, 1000);

            lastFileName = System.IO.Path.Combine(outputFolder, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".avi");
            var bitRate = Mp3AudioEncoderLame.SupportedBitRates.OrderBy(br => br).ElementAt(audioQuality);
            recorder = new Recorder(lastFileName, 
                encoder, encodingQuality, 
                audioSourceIndex, audioWaveFormat, encodeAudio, bitRate);
        }

        private void StopRecording()
        {
            if (!IsRecording)
                throw new InvalidOperationException("Not recording.");

            recorder.Dispose();
            recorder = null;

            recordingTimer.Change(0, 0);

            IsRecording = false;
            HasLastScreencast = true;

            WindowState = WindowState.Normal;
        }

        private void recordingTimer_Tick(object state)
        {
            var elapsed = DateTime.Now - recordingStartTime;
            var elapsedString = string.Format("{0:00}:{1:00}", Math.Floor(elapsed.TotalMinutes), elapsed.Seconds);
            Dispatcher.BeginInvoke(new Action(() => { Elapsed = elapsedString; }));
        }

        #endregion


        #region Settings

        private string outputFolder;
        private FourCC encoder;
        private int encodingQuality;
        private int audioSourceIndex;
        private SupportedWaveFormat audioWaveFormat;
        private bool encodeAudio;
        private int audioQuality;
        private bool minimizeOnStart;

        private void InitDefaultSettings()
        {
            var exePath = new Uri(System.Reflection.Assembly.GetEntryAssembly().Location).LocalPath;
            outputFolder = System.IO.Path.GetDirectoryName(exePath);

            encoder = KnownFourCCs.Codecs.MotionJpeg;
            encodingQuality = 70;

            audioSourceIndex = -1;
            audioWaveFormat = SupportedWaveFormat.WAVE_FORMAT_44M16;
            encodeAudio = true;
            audioQuality = (Mp3AudioEncoderLame.SupportedBitRates.Length + 1) / 2;

            minimizeOnStart = true;
        }

        private void ShowSettingsDialog()
        {
            var dlg = new SettingsWindow()
            {
                Owner = this,
                Folder = outputFolder,
                Encoder = encoder,
                Quality = encodingQuality,
                SelectedAudioSourceIndex = audioSourceIndex,
                AudioWaveFormat = audioWaveFormat,
                EncodeAudio = encodeAudio,
                AudioQuality = audioQuality,
                MinimizeOnStart = minimizeOnStart
            };
            
            if (dlg.ShowDialog() == true)
            {
                outputFolder = dlg.Folder;
                encoder = dlg.Encoder;
                encodingQuality = dlg.Quality;
                audioSourceIndex = dlg.SelectedAudioSourceIndex;
                audioWaveFormat = dlg.AudioWaveFormat;
                encodeAudio = dlg.EncodeAudio;
                audioQuality = dlg.AudioQuality;
                minimizeOnStart = dlg.MinimizeOnStart;
            }
        }

        #endregion


        private void StartRecording_Click(object sender, RoutedEventArgs e)
        {
            StartRecording();
        }

        private void StopRecording_Click(object sender, RoutedEventArgs e)
        {
            StopRecording();
        }

        private void GoToLastScreencast_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", string.Format("/select, \"{0}\"", lastFileName));
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            ShowSettingsDialog();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (IsRecording)
                StopRecording();

            Close();
        }
    }
}
