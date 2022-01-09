using SharpAvi.Utilities;
using System;
using System.Threading.Tasks;

namespace SharpAvi.Codecs
{
    /// <summary>
    /// Ensures that all access to the enclosed <see cref="IVideoEncoder"/> instance is made
    /// on a single thread.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Especially useful for unmanaged encoders like <see cref="Mpeg4VcmVideoEncoder"/> in multi-threaded scenarios
    /// like asynchronous encoding.
    /// </para>
    /// </remarks>
    public class SingleThreadedVideoEncoderWrapper : IVideoEncoder, IDisposable
    {
        private readonly IVideoEncoder encoder;
        private readonly SingleThreadTaskScheduler scheduler;

        /// <summary>
        /// Creates a new instance of <see cref="SingleThreadedVideoEncoderWrapper"/>.
        /// </summary>
        /// <param name="encoderFactory">
        /// Factory for creating an encoder instance.
        /// It will be invoked on the same thread as all subsequent operations of the <see cref="IVideoEncoder"/> interface.
        /// </param>
        public SingleThreadedVideoEncoderWrapper(Func<IVideoEncoder> encoderFactory)
        {
            Argument.IsNotNull(encoderFactory, nameof(encoderFactory));

            scheduler = new SingleThreadTaskScheduler();

            // TODO: Create encoder on the first frame
            encoder = SchedulerInvoke(encoderFactory);
            if (encoder is null)
            {
                throw new InvalidOperationException("Encoder factory has created no instance.");
            }
        }

        /// <summary>
        /// Disposes the enclosed encoder and stops the internal thread.
        /// </summary>
        public void Dispose()
        {
            if (!scheduler.IsDisposed)
            {
                if (encoder is IDisposable disposable)
                {
                    new Task(disposable.Dispose).RunSynchronously(scheduler);
                }
                scheduler.Dispose();
            }
        }

        /// <summary>Codec ID.</summary>
        public FourCC Codec
        {
            get
            {
                return SchedulerInvoke(() => encoder.Codec);
            }
        }

        /// <summary>
        /// Number of bits per pixel in encoded image.
        /// </summary>
        public BitsPerPixel BitsPerPixel
        {
            get
            {
                return SchedulerInvoke(() => encoder.BitsPerPixel);
            }
        }

        /// <summary>
        /// Determines the amount of space needed in the destination buffer for storing the encoded data of a single frame.
        /// </summary>
        public int MaxEncodedSize
        {
            get
            {
                return SchedulerInvoke(() => encoder.MaxEncodedSize);
            }
        }

        /// <summary>
        /// Encodes a video frame.
        /// </summary>
        public int EncodeFrame(byte[] source, int srcOffset, byte[] destination, int destOffset, out bool isKeyFrame)
        {
            var result = SchedulerInvoke(
                () => EncodeFrame(source, srcOffset, destination, destOffset));
            isKeyFrame = result.IsKeyFrame;
            return result.EncodedLength;
        }

        private EncodeResult EncodeFrame(byte[] source, int srcOffset, byte[] destination, int destOffset)
        {
            bool isKeyFrame;
            var result = encoder.EncodeFrame(source, srcOffset, destination, destOffset, out isKeyFrame);
            return new EncodeResult
            {
                EncodedLength = result,
                IsKeyFrame = isKeyFrame
            };
        }

#if NET5_0_OR_GREATER
        /// <summary>
        /// Encodes a video frame.
        /// </summary>
        public unsafe int EncodeFrame(ReadOnlySpan<byte> source, Span<byte> destination, out bool isKeyFrame)
        {
            EncodeResult result;
            fixed (void* srcPtr = source, destPtr = destination)
            {
                var srcIntPtr = new IntPtr(srcPtr);
                var srcLength = source.Length;
                var destIntPtr = new IntPtr(destPtr);
                var destLength = destination.Length;
                result = SchedulerInvoke(
                    () => EncodeFrame(srcIntPtr, srcLength, destIntPtr, destLength));
            }
            isKeyFrame = result.IsKeyFrame;
            return result.EncodedLength;
        }

        private unsafe EncodeResult EncodeFrame(IntPtr source, int srcLength, IntPtr destination, int destLength)
        {
            bool isKeyFrame;
            var sourceSpan = new Span<byte>(source.ToPointer(), srcLength);
            var destSpan = new Span<byte>(destination.ToPointer(), destLength);
            var result = encoder.EncodeFrame(sourceSpan, destSpan, out isKeyFrame);
            return new EncodeResult
            {
                EncodedLength = result,
                IsKeyFrame = isKeyFrame
            };
        }
#endif

        private struct EncodeResult
        {
            public int EncodedLength;
            public bool IsKeyFrame;
        }


        private TResult SchedulerInvoke<TResult>(Func<TResult> func)
        {
            var task = new Task<TResult>(func);
            task.RunSynchronously(scheduler);
            return task.Result;
        }
    }
}