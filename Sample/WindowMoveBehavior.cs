using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            window.Closed += Window_Closed;
            window.MouseLeftButtonDown += Window_MouseLeftButtonDown;
            window.MouseLeftButtonUp += Window_MouseLeftButtonUp;
            window.MouseMove += Window_MouseMove;
        }

        private static void Window_Closed(object sender, EventArgs e)
        {
            var window = (Window)sender;
            window.Closed -= Window_Closed;
            window.MouseLeftButtonDown -= Window_MouseLeftButtonDown;
            window.MouseLeftButtonUp -= Window_MouseLeftButtonUp;
            window.MouseMove -= Window_MouseMove;
        }

        private static void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var window = (Window)sender;
            window.SetValue(MoveOriginProperty, e.GetPosition(window));
            window.CaptureMouse();
        }

        private static void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var window = (Window)sender;
            if (window.IsMouseCaptured)
            {
                window.ReleaseMouseCapture();
            }
        }

        private static void Window_MouseMove(object sender, MouseEventArgs e)
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
