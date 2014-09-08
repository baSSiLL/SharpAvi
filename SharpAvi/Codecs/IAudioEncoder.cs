using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpAvi.Output;

namespace SharpAvi.Codecs
{
    public interface IAudioEncoder
    {
        /// <summary>
        /// Gets the maximum number of bytes in encoded data for a given number of source bytes.
        /// </summary>
        /// <param name="sourceCount">Number of source bytes.</param>
        int GetMaxEncodedLength(int sourceCount);

        /// <summary>
        /// Encodes block of audio data.
        /// </summary>
        /// <param name="source">Buffer with audio data.</param>
        /// <param name="sourceOffset">Offset to start reading <paramref name="source"/>.</param>
        /// <param name="sourceCount">Number of bytes to read from <paramref name="source"/>.</param>
        /// <param name="destination">Buffer for encoded audio data.</param>
        /// <param name="destinationOffset">Offset to start writing to <paramref name="destination"/>.</param>
        /// <returns>The number of bytes written to <paramref name="destination"/>.</returns>
        int EncodeBlock(byte[] source, int sourceOffset, int sourceCount, byte[] destination, int destinationOffset);

        /// <summary>
        /// Initializes audio stream before writing starts.
        /// </summary>
        /// <param name="stream">A stream to initialize.</param>
        /// <remarks>
        /// An encoder should set correct values for properties of the stream used in
        /// headers of AVI file. 
        /// It SHOULD NOT write any data to the stream.
        /// </remarks>
        void InitializeStream(IAviAudioStream stream);
    }
}
