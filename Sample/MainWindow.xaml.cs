using System;
using System.Windows;
using System.Windows.Input;
using System.Threading;

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

            DataContext = this;
        }


        #region Recording

        private DateTime recordingStartTime;
        private Thread recordingThread;
        private ManualResetEvent stopRecordingThread = new ManualResetEvent(false);

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

        private void StartRecording()
        {
            if (IsRecording)
                throw new InvalidOperationException("Already recording.");

            Elapsed = "00:00";
            IsRecording = true;

            recordingThread = new Thread(Record)
            {
                Name = "Record",
                IsBackground = true
            };
            recordingStartTime = DateTime.Now;
            stopRecordingThread.Reset();
            recordingThread.Start();
        }

        private void StopRecording()
        {
            if (!IsRecording)
                throw new InvalidOperationException("Not recording.");

            if (recordingThread.IsAlive)
            {
                stopRecordingThread.Set();
                recordingThread.Join();
            }
            recordingThread = null;

            IsRecording = false;
        }

        private void Record()
        {
            while (!stopRecordingThread.WaitOne(500))
            {
                var elapsed = DateTime.Now - recordingStartTime;
                var elapsedString = string.Format("{0:00}:{1:00}", Math.Floor(elapsed.TotalMinutes), elapsed.Seconds);
                Dispatcher.BeginInvoke(new Action(() => { Elapsed = elapsedString; }));
            }
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

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (IsRecording)
                StopRecording();

            Close();
        }
    }
}
