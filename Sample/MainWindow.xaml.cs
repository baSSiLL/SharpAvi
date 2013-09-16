using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Input;

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

            Elapsed = "00:00";
            HasLastScreencast = false;
            IsRecording = true;

            recordingStartTime = DateTime.Now;
            recordingTimer.Change(1000, 1000);

            lastFileName = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".avi";
            recorder = new Recorder(lastFileName);
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
        }

        private void recordingTimer_Tick(object state)
        {
            var elapsed = DateTime.Now - recordingStartTime;
            var elapsedString = string.Format("{0:00}:{1:00}", Math.Floor(elapsed.TotalMinutes), elapsed.Seconds);
            Dispatcher.BeginInvoke(new Action(() => { Elapsed = elapsedString; }));
        }

        #endregion


        #region Moving Window

        private Point moveOrigin;

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            moveOrigin = e.GetPosition(this);
            CaptureMouse();
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (IsMouseCaptured)
            {
                ReleaseMouseCapture();
            }

            base.OnMouseLeftButtonUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (IsMouseCaptured)
            {
                var offset = e.GetPosition(this) - moveOrigin;
                Left += offset.X;
                Top += offset.Y;
            }
        }

        #endregion


        #region Window opacity

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            Opacity = 1;
        }

        protected override void OnDeactivated(EventArgs e)
        {
            Opacity = 0.5;

            base.OnDeactivated(e);
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

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (IsRecording)
                StopRecording();

            Close();
        }
    }
}
