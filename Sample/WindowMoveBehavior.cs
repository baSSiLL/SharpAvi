using System;
using System.Windows;
using System.Windows.Input;

namespace SharpAvi.Sample
{
    internal static class WindowMoveBehavior
    {
        private static readonly DependencyProperty MoveOriginProperty =
            DependencyProperty.RegisterAttached("MoveOrigin", typeof(Point), typeof(WindowMoveBehavior));

        public static void Attach(Window window)
        {
            window.Closed += window_Closed;
            window.MouseLeftButtonDown += window_MouseLeftButtonDown;
            window.MouseLeftButtonUp += window_MouseLeftButtonUp;
            window.MouseMove += window_MouseMove;
        }

        private static void window_Closed(object sender, EventArgs e)
        {
            var window = (Window)sender;
            window.Closed -= window_Closed;
            window.MouseLeftButtonDown -= window_MouseLeftButtonDown;
            window.MouseLeftButtonUp -= window_MouseLeftButtonUp;
            window.MouseMove -= window_MouseMove;
        }

        private static void window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var window = (Window)sender;
            window.SetValue(MoveOriginProperty, e.GetPosition(window));
            window.CaptureMouse();
        }

        private static void window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var window = (Window)sender;
            if (window.IsMouseCaptured)
            {
                window.ReleaseMouseCapture();
            }
        }

        private static void window_MouseMove(object sender, MouseEventArgs e)
        {
            var window = (Window)sender;
            if (window.IsMouseCaptured)
            {
                var offset = e.GetPosition(window) - (Point)window.GetValue(MoveOriginProperty);
                window.Left += offset.X;
                window.Top += offset.Y;
            }
        }
    }
}
