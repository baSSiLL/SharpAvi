using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace SharpAvi.Output
{
    /// <summary>
    /// Audio stream of AVI file.
    /// </summary>
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
        /// Number of bits per sample per single channel (usually 8 or 16).
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
        /// Average byte rate of the stream.
        /// </summary>
        int BytesPerSecond { get; set; }

        /// <summary>
        /// Size in bytes of minimum item of data in the stream.
        /// </summary>
        /// <remarks>
        /// Corresponds to <c>nBlockAlign</c> field of <c>WAVEFORMATEX</c> structure.
        /// </remarks>
        int Granularity { get; set; }

        /// <summary>
        /// Extra data defined by a specific format which should be added to the stream header.
        /// </summary>
        /// <remarks>
        /// Contains data of specific structure like <c>MPEGLAYER3WAVEFORMAT</c> that follow
        /// common <c>WAVEFORMATEX</c> field.
        /// </remarks>
        byte[] FormatSpecificData { get; set; }

        /// <summary>
        /// Writes a block of audio data.
        /// </summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="startIndex">Start index of data.</param>
        /// <param name="length">Length of data.</param>
        /// <remarks>
        /// Division of audio data into blocks may be arbitrary.
        /// However, it is reasonable to write blocks of approximately the same duration
        /// as a single video frame.
        /// </remarks>
        void WriteBlock(byte[] data, int startIndex, int length);

        /// <summary>
        /// Asynchronously writes a block of audio data.
        /// </summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="startIndex">Start index of data.</param>
        /// <param name="length">Length of data.</param>
        /// <returns>
        /// A task representing the asynchronous write operation.
        /// </returns>
        /// <remarks>
        /// Division of audio data into blocks may be arbitrary.
        /// However, it is reasonable to write blocks of approximately the same duration
        /// as a single video frame.
        /// The contents of <paramref name="data"/> should not be modified until this write operation ends.
        /// </remarks>
        Task WriteBlockAsync(byte[] data, int startIndex, int length);

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

            public int BytesPerSecond
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

            public int Granularity
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

            public Task WriteBlockAsync(byte[] data, int startIndex, int length)
            {
                Contract.Requires(data != null);
                Contract.Requires(startIndex >= 0);
                Contract.Requires(length >= 0);
                Contract.Requires(startIndex + length <= data.Length);
                Contract.Ensures(Contract.Result<Task>() != null);
                throw new NotImplementedException();
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
