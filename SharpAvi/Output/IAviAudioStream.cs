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
