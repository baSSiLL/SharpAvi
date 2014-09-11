using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpAvi.Codecs
{
    partial class Mp3AudioEncoderLame
    {
        /// <summary>
        /// Interface is used to access the API of the LAME DLL.
        /// </summary>
        /// <remarks>
        /// Clients of <see cref="Mp3AudioEncoderLame"/> class need not to work with
        /// this interface directly.
        /// </remarks>
        public interface ILameFacade
        {
            /// <summary>
            /// Number of audio channels.
            /// </summary>
            int ChannelCount { get; set; }

            /// <summary>
            /// Sample rate of source audio data.
            /// </summary>
            int InputSampleRate { get; set; }

            /// <summary>
            /// Bit rate of encoded data.
            /// </summary>
            int OutputBitRate { get; set; }

            /// <summary>
            /// Sample rate of encoded data.
            /// </summary>
            int OutputSampleRate { get; }

            /// <summary>
            /// Frame size of encoded data.
            /// </summary>
            int FrameSize { get; }

            /// <summary>
            /// Encoder delay.
            /// </summary>
            int EncoderDelay { get; }

            /// <summary>
            /// Initializes the encoding process.
            /// </summary>
            void PrepareEncoding();

            /// <summary>
            /// Encodes a chunk of audio data.
            /// </summary>
            int Encode(byte[] source, int sourceIndex, int sampleCount, byte[] dest, int destIndex);

            /// <summary>
            /// Finalizes the encoding process.
            /// </summary>
            int FinishEncoding(byte[] dest, int destIndex);
        }
    }
}
