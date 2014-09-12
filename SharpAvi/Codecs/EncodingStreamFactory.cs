using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using SharpAvi.Output;

namespace SharpAvi.Codecs
{
    /// <summary>
    /// Provides extension methods for creating encoding streams with specific encoders.
    /// </summary>
    public static class EncodingStreamFactory
    {
        /// <summary>
        /// Adds new video stream with <see cref="UncompressedVideoEncoder"/>.
        /// </summary>
        /// <seealso cref="AviWriter.AddEncodingVideoStream"/>
        /// <seealso cref="UncompressedVideoEncoder"/>
        public static IAviVideoStream AddUncompressedVideoStream(this AviWriter writer, int width, int height)
        {
            Contract.Requires(writer != null);
            Contract.Requires(width > 0);
            Contract.Requires(height > 0);
            Contract.Ensures(Contract.Result<IAviVideoStream>() != null);

            var encoder = new UncompressedVideoEncoder(width, height);
            return writer.AddEncodingVideoStream(encoder, true, width, height);
        }

#if FX45
        /// <summary>
        /// Adds new video stream with <see cref="MotionJpegVideoEncoderWpf"/>.
        /// </summary>
        /// <param name="writer">Writer object to which new stream is added.</param>
        /// <param name="width">Frame width.</param>
        /// <param name="height">Frame height.</param>
        /// <param name="quality">Requested quality of compression.</param>
        /// <seealso cref="AviWriter.AddEncodingVideoStream"/>
        /// <seealso cref="MotionJpegVideoEncoderWpf"/>
#else
        /// <summary>
        /// Adds new video stream with <see cref="MotionJpegVideoEncoderWpf"/>.
        /// </summary>
        /// <param name="writer">Writer object to which new stream is added.</param>
        /// <param name="width">Frame width.</param>
        /// <param name="height">Frame height.</param>
        /// <param name="quality">Requested quality of compression.</param>
        /// <param name="forceSingleThreadedAccess">
        /// When <c>true</c>, the created <see cref="MotionJpegVideoEncoderWpf"/> instance is wrapped into
        /// <see cref="SingleThreadedVideoEncoderWrapper"/>.
        /// </param>
        /// <seealso cref="AviWriter.AddEncodingVideoStream"/>
        /// <seealso cref="MotionJpegVideoEncoderWpf"/>
#endif
        public static IAviVideoStream AddMotionJpegVideoStream(this AviWriter writer, int width, int height, int quality = 70
#if !FX45
            , bool forceSingleThreadedAccess = false
#endif
            )
        {
            Contract.Requires(writer != null);
            Contract.Requires(width > 0);
            Contract.Requires(height > 0);
            Contract.Requires(1 <= quality && quality <= 100);
            Contract.Ensures(Contract.Result<IAviVideoStream>() != null);

#if FX45
            var encoder = new MotionJpegVideoEncoderWpf(width, height, quality);
#else
            var encoderFactory = new Func<IVideoEncoder>(() => new MotionJpegVideoEncoderWpf(width, height, quality));
            var encoder = forceSingleThreadedAccess
                ? new SingleThreadedVideoEncoderWrapper(encoderFactory)
                : encoderFactory.Invoke();
#endif
            return writer.AddEncodingVideoStream(encoder, true, width, height);
        }

        /// <summary>
        /// Adds new video stream with <see cref="Mpeg4VideoEncoderVcm"/>.
        /// </summary>
        /// <param name="writer">Writer object to which new stream is added.</param>
        /// <param name="width">Frame width.</param>
        /// <param name="height">Frame height.</param>
        /// <param name="fps">Frames rate of the video.</param>
        /// <param name="frameCount">Number of frames if known in advance. Otherwise, specify <c>0</c>.</param>
        /// <param name="quality">Requested quality of compression.</param>
        /// <param name="codec">Specific MPEG-4 codec to use.</param>
        /// <param name="forceSingleThreadedAccess">
        /// When <c>true</c>, the created <see cref="Mpeg4VideoEncoderVcm"/> instance is wrapped into
        /// <see cref="SingleThreadedVideoEncoderWrapper"/>.
        /// </param>
        /// <seealso cref="AviWriter.AddEncodingVideoStream"/>
        /// <seealso cref="Mpeg4VideoEncoderVcm"/>
        /// <seealso cref="SingleThreadedVideoEncoderWrapper"/>
        public static IAviVideoStream AddMpeg4VideoStream(this AviWriter writer, int width, int height, 
            double fps, int frameCount = 0, int quality = 70, FourCC? codec = null, 
            bool forceSingleThreadedAccess = false)
        {
            Contract.Requires(writer != null);
            Contract.Requires(width > 0);
            Contract.Requires(height > 0);
            Contract.Requires(fps > 0);
            Contract.Requires(frameCount >= 0);
            Contract.Requires(1 <= quality && quality <= 100);
            Contract.Ensures(Contract.Result<IAviVideoStream>() != null);

            var encoderFactory = codec.HasValue
                ? new Func<IVideoEncoder>(() => new Mpeg4VideoEncoderVcm(width, height, fps, frameCount, quality, codec.Value))
                : new Func<IVideoEncoder>(() => new Mpeg4VideoEncoderVcm(width, height, fps, frameCount, quality));
            var encoder = forceSingleThreadedAccess
                ? new SingleThreadedVideoEncoderWrapper(encoderFactory)
                : encoderFactory.Invoke();
            return writer.AddEncodingVideoStream(encoder, true, width, height);
        }

        /// <summary>
        /// Adds new audio stream with <see cref="Mp3AudioEncoderLame"/>.
        /// </summary>
        /// <seealso cref="AviWriter.AddEncodingAudioStream"/>
        /// <seealso cref="Mp3AudioEncoderLame"/>
        public static IAviAudioStream AddMp3AudioStream(this AviWriter writer, int channelCount, int sampleRate, int outputBitRateKbps = 160)
        {
            Contract.Requires(writer != null);
            Contract.Requires(channelCount == 1 || channelCount == 2);
            Contract.Requires(sampleRate > 0);
            Contract.Requires(Mp3AudioEncoderLame.SupportedBitRates.Contains(outputBitRateKbps));
            Contract.Ensures(Contract.Result<IAviAudioStream>() != null);

            var encoder = new Mp3AudioEncoderLame(channelCount, sampleRate, outputBitRateKbps);
            return writer.AddEncodingAudioStream(encoder, true);
        }
    }
}
