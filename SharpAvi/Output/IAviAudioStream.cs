using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace SharpAvi.Output
{
    [ContractClass(typeof(Contracts.AviAudioStreamContract))]
    public interface IAviAudioStream : IAviStream
    {
        /// <summary>
        /// Number of channels in this audio stream.
        /// </summary>
        /// <remarks>
        /// For example, <c>1</c> for mono and <c>2</c> for stereo.
        /// </remarks>
        int ChannelCount { get; set; }

        /// <summary>
        /// Sample rate, in samples per second (herz).
        /// </summary>
        int SamplesPerSecond { get; set; }

        /// <summary>
        /// Number of bits per sample (usually 8 or 16).
        /// </summary>
        int BitsPerSample { get; set; }

        /// <summary>
        /// Format of the audio data.
        /// </summary>
        /// <remarks>
        /// The formats are defined in <c>mmreg.h</c> from Windows SDK.
        /// Some of the well-known formats are listed in the <see cref="AudioFormats"/> class.
        /// </remarks>
        short Format { get; set; }

        /// <summary>
        /// Extra data defined by a specific format which should be added to the stream header.
        /// </summary>
        byte[] FormatSpecificData { get; set; }

        /// <summary>
        /// Writes a block of audio data.
        /// </summary>
        /// <remarks>
        /// Division of audio data into blocks may be arbitrary.
        /// However, it is reasonable to write blocks of approximately the same duration
        /// as a single video frame.
        /// </remarks>
        /// <param name="data">Data buffer.</param>
        /// <param name="startIndex">Start index of data.</param>
        /// <param name="length">Length of data.</param>
        void WriteBlock(byte[] data, int startIndex, int length);

        /// <summary>
        /// Number of blocks written.
        /// </summary>
        int BlocksWritten { get; }
    }

    
    namespace Contracts
    {
        [ContractClassFor(typeof(IAviAudioStream))]
        internal abstract class AviAudioStreamContract : IAviAudioStream
        {
            public int ChannelCount
            {
                get
                {
                    Contract.Ensures(Contract.Result<int>() > 0);
                    throw new NotImplementedException();
                }
                set
                {
                    Contract.Requires(value > 0);
                }
            }

            public int SamplesPerSecond
            {
                get
                {
                    Contract.Ensures(Contract.Result<int>() > 0);
                    throw new NotImplementedException();
                }
                set
                {
                    Contract.Requires(value > 0);
                }
            }

            public int BitsPerSample
            {
                get
                {
                    Contract.Ensures(Contract.Result<int>() > 0);
                    throw new NotImplementedException();
                }
                set
                {
                    Contract.Requires(value > 0);
                }
            }

            public short Format
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                }
            }

            public byte[] FormatSpecificData
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                }
            }

            public void WriteBlock(byte[] data, int startIndex, int length)
            {
                Contract.Requires(data != null);
                Contract.Requires(startIndex >= 0);
                Contract.Requires(length >= 0);
                Contract.Requires(startIndex + length <= data.Length);
            }

            public int BlocksWritten
            {
                get 
                {
                    Contract.Ensures(Contract.Result<int>() >= 0);
                    throw new NotImplementedException();
                }
            }

            public int Index
            {
                get { throw new NotImplementedException(); }
            }

            public string Name
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                }
            }
        }
    }
}
