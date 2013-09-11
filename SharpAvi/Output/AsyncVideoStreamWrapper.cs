using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;

namespace SharpAvi.Output
{
    /// <summary>
    /// Adds to video stream the ability to write frames asynchronously.
    /// </summary>
    public class AsyncVideoStreamWrapper : IAviVideoStream, IDisposable
    {
        private readonly IAviVideoStream stream;
        private bool isDisposed;

        private readonly object sync = new object();
        private WriteFrameArgs writeFrameArgs;
        private Exception writeException;
        private readonly Thread writingThread;
        private readonly AutoResetEvent stopAwaiting = new AutoResetEvent(false);
        private readonly AutoResetEvent writeFrameAwaiting = new AutoResetEvent(false);
        private readonly ManualResetEvent frameWritten = new ManualResetEvent(true);

        /// <summary>
        /// Creates a new instance of <see cref="AsyncVideoStreamWrapper"/>.
        /// </summary>
        /// <param name="stream">Stream to be warpped.</param>
        /// <remarks>
        /// The constructor starts a thread for performing asynchronous writing.
        /// </remarks>
        public AsyncVideoStreamWrapper(IAviVideoStream stream)
        {
            Contract.Requires(stream != null);

            this.stream = stream;

            writingThread = new Thread(WritingLoop)
            {
                Name = typeof(AsyncVideoStreamWrapper).Name,
                IsBackground = true
            };
            writingThread.Start();
        }

        /// <summary>
        /// Disposes all unmanaged resources.
        /// </summary>
        /// <remarks>
        /// This method also stops the thread used for asynchronous writing.
        /// </remarks>
        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;

                if (writingThread != null && writingThread.IsAlive)
                {
                    stopAwaiting.Set();
                    if (!writingThread.Join(3000))
                    {
                        writingThread.Abort();
                    }
                }

                // TODO: Dispose underlying stream if it implements IDisposable?
            }
        }

        private void CheckNotDisposed()
        {
            if (isDisposed)
                throw new ObjectDisposedException(typeof(AsyncVideoStreamWrapper).Name);
        }


        #region IAviStream implementation

        /// <summary>Sequential number of the stream.</summary>
        public int Index
        {
            get { return stream.Index; }
        }

        /// <summary>Name of the stream.</summary>
        public string Name
        {
            get { return stream.Name; }
            set { stream.Name = value; }
        }

        /// <summary>Codec.</summary>
        public FourCC Codec
        {
            get { return stream.Codec; }
            set { stream.Codec = value; }
        }

        #endregion


        #region IAviVideoStream implementation

        /// <summary>Frame width.</summary>
        public int Width
        {
            get { return stream.Width; }
            set { stream.Width = value; }
        }

        /// <summary>Frame height.</summary>
        public int Height
        {
            get { return stream.Height; }
            set { stream.Height = value; }
        }

        /// <summary>
        /// Number of bits per pixel in frame's image.
        /// </summary>
        public BitsPerPixel BitsPerPixel
        {
            get { return stream.BitsPerPixel; }
            set { stream.BitsPerPixel = value; }
        }

        /// <summary>
        /// Synchronously writes a frame.
        /// </summary>
        /// <seealso cref="IAviVideoStream.WriteFrame"/>
        public void WriteFrame(bool isKeyFrame, byte[] frameData, int startIndex, int count)
        {
            BeginWriteFrame(isKeyFrame, frameData, startIndex, count);
            EndWriteFrame();
        }

        #endregion


        #region Asynchronous writing

        /// <summary>Asynchronously writes the next frame.</summary>
        /// <remarks>
        /// Call <see cref="EndWriteFrame"/> before writing a new frame.
        /// Accessing an array passed as the <paramref name="frameData"/> is not safe until <see cref="EndWriteFrame"/> is invoked.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Writing the previous frame resulted in error. Further use of this instance is not possible.
        /// The <see cref="Exception.InnerException"/> property contains the original exception.
        /// </exception>
        /// <seealso cref="IAviVideoStream.WriteFrame"/>
        public void BeginWriteFrame(bool isKeyFrame, byte[] frameData, int startIndex, int count)
        {
            Contract.Requires(frameData != null);
            Contract.Requires(startIndex >= 0);
            Contract.Requires(count >= 0);
            Contract.Requires(startIndex + count <= frameData.Length);
            
            CheckNotDisposed();

            frameWritten.WaitOne();

            lock (sync)
            {
                if (writeException != null)
                {
                    throw new InvalidOperationException("Writing previous frame resulted in error. Further use of this instance is not possible.", writeException);
                }
                frameWritten.Reset();
                writeFrameArgs = new WriteFrameArgs
                    {
                        IsKeyFrame = isKeyFrame,
                        FrameData = frameData,
                        StartIndex = startIndex,
                        Count = count
                    };
                writeFrameAwaiting.Set();
            }
        }

        /// <summary>
        /// Waits until the frame is written after the previous invocation of <see cref="BeginWriteFrame"/>.
        /// </summary>
        /// <exception cref="IOException">
        /// There was an exception when writing a frame. Further use of this instance is not possible.
        /// The <see cref="Exception.InnerException"/> property contains the original exception.
        /// </exception>
        public void EndWriteFrame()
        {
            CheckNotDisposed();

            frameWritten.WaitOne();

            lock (sync)
            {
                if (writeException != null)
                {
                    throw new IOException("Writing frame resulted in error.", writeException);
                }
            }
        }

        private void WritingLoop()
        {
            try
            {
                var events = new WaitHandle[] { stopAwaiting, writeFrameAwaiting };
                var stop = false;
                while (!stop)
                {
                    var incomingEvent = events[WaitHandle.WaitAny(events)];
                    if (incomingEvent == writeFrameAwaiting)
                    {
                        bool isKeyFrame;
                        byte[] frameData;
                        int startIndex;
                        int count;
                        lock (sync)
                        {
                            isKeyFrame = writeFrameArgs.IsKeyFrame;
                            frameData = writeFrameArgs.FrameData;
                            startIndex = writeFrameArgs.StartIndex;
                            count = writeFrameArgs.Count;
                        }

                        stream.WriteFrame(isKeyFrame, frameData, startIndex, count);
                        frameWritten.Set();
                    }
                    else
                    {
                        stop = true;
                    }
                }
            }
            catch (Exception ex)
            {
                lock (sync)
                {
                    writeException = ex;
                }
                frameWritten.Set();
            }
        }


        private struct WriteFrameArgs
        {
            public bool IsKeyFrame;
            public byte[] FrameData;
            public int StartIndex;
            public int Count;
        }

        #endregion
    }
}
