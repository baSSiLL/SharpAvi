using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using SharpAvi.Codecs;
using SharpAvi.Output;

namespace SharpAvi.Sample
{
    internal class Recorder : IDisposable
    {
        private readonly int screenWidth;
        private readonly int screenHeight;
        private readonly AviWriter writer;
        private readonly IVideoEncoder encoder;
        private readonly AsyncVideoStreamWrapper stream;
        private readonly Thread thread;
        private readonly ManualResetEvent stopThread = new ManualResetEvent(false);

        public Recorder(string fileName)
        {
            screenWidth = (int)SystemParameters.PrimaryScreenWidth;
            screenHeight = (int)SystemParameters.PrimaryScreenHeight;

            // Create AVI writer and specify FPS
            writer = new AviWriter(fileName)
            {
                FramesPerSecond = 10
            };

            // Create video encoder
            encoder = new MotionJpegVideoEncoderWpf(screenWidth, screenHeight, 70);

            // Create video stream, wrapping it for encoding and asynchronous operations
            stream = writer.AddVideoStream().WithEncoder(encoder).Async();

            // Set video stream parameters (BitsPerPixel and Codec are internally set by the encoder used)
            stream.Name = "Screencast";
            stream.Width = screenWidth;
            stream.Height = screenHeight;

            thread = new Thread(Record)
            {
                Name = typeof(Recorder).Name + ".Record",
                IsBackground = true
            };
            thread.Start();
        }

        public void Dispose()
        {
            stopThread.Set();
            thread.Join();

            // Close writer: the remaining data is written to a file and file is closed
            writer.Close();

            stopThread.Close();
            var encoderDisposable = encoder as IDisposable;
            if (encoderDisposable != null)
            {
                encoderDisposable.Dispose();
            }
        }

        private void Record()
        {
            var frameInterval = TimeSpan.FromSeconds(1 / (double)writer.FramesPerSecond);
            var buffer = new byte[screenWidth * screenHeight * 4];
            var isFirstFrame = true;
            var timeTillNextFrame = TimeSpan.Zero;
            while (!stopThread.WaitOne(timeTillNextFrame))
            {
                var timestamp = DateTime.Now;

                GetScreenshot(buffer);

                // Wait for the previous frame is written
                if (!isFirstFrame)
                {
                    stream.EndWriteFrame();
                }

                // Start asynchronous (encoding and) writing of the new frame
                stream.BeginWriteFrame(true, buffer, 0, buffer.Length);

                timeTillNextFrame = timestamp + frameInterval - DateTime.Now;
                if (timeTillNextFrame < TimeSpan.Zero)
                    timeTillNextFrame = TimeSpan.Zero;

                isFirstFrame = false;
            }

            // Wait for the last frame is written
            if (!isFirstFrame)
            {
                stream.EndWriteFrame();
            }
        }

        private void GetScreenshot(byte[] buffer)
        {
            using (var bitmap = new Bitmap(screenWidth, screenHeight))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(0, 0, 0, 0, new System.Drawing.Size(screenWidth, screenHeight));
                var bits = bitmap.LockBits(new Rectangle(0, 0, screenWidth, screenHeight), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
                Marshal.Copy(bits.Scan0, buffer, 0, buffer.Length);
                bitmap.UnlockBits(bits);

                // Should also capture the mouse cursor here, but skipping for simplicity
                // For those who are interested, look at http://www.codeproject.com/Articles/12850/Capturing-the-Desktop-Screen-with-the-Mouse-Cursor
            }
        }
    }
}
