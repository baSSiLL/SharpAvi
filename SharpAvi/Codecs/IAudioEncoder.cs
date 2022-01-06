namespace SharpAvi.Codecs
{
    /// <summary>
    /// Encoder of audio streams.
    /// </summary>
    public interface IAudioEncoder
    {
        /// <summary>
        /// Number of channels in encoded audio.
        /// </summary>
        int ChannelCount { get; }

        /// <summary>
        /// Sample rate of encoded audio, in samples per second.
        /// </summary>
        int SamplesPerSecond { get; }

        /// <summary>
        /// Number of bits per sample per single channel in encoded audio (usually 8 or 16).
        /// </summary>
        int BitsPerSample { get; }

        /// <summary>
        /// Format of encoded audio.
        /// </summary>
        short Format { get; }

        /// <summary>
        /// Byte rate of encoded audio, in bytes per second.
        /// </summary>
        int BytesPerSecond { get; }

        /// <summary>
        /// Size in bytes of minimum item of encoded data.
        /// </summary>
        /// <remarks>
        /// Corresponds to <c>nBlockAlign</c> field of <c>WAVEFORMATEX</c> structure.
        /// </remarks>
        int Granularity { get; }

        /// <summary>
        /// Extra data defined by a specific format which should be added to the stream header.
        /// </summary>
        /// <remarks>
        /// Contains data of specific structure like <c>MPEGLAYER3WAVEFORMAT</c> that follow
        /// common <c>WAVEFORMATEX</c> field.
        /// </remarks>
        byte[] FormatSpecificData { get; }

        /// <summary>
        /// Gets the maximum number of bytes in encoded data for a given number of source bytes.
        /// </summary>
        /// <param name="sourceCount">Number of source bytes. Specify <c>0</c> for a flush buffer size.</param>
        /// <seealso cref="EncodeBlock"/>
        /// <seealso cref="Flush"/>
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
        /// <seealso cref="GetMaxEncodedLength"/>
        int EncodeBlock(byte[] source, int sourceOffset, int sourceCount, byte[] destination, int destinationOffset);

        /// <summary>
        /// Flushes internal encoder buffers if any.
        /// </summary>
        /// <param name="destination">Buffer for encoded audio data.</param>
        /// <param name="destinationOffset">Offset to start writing to <paramref name="destination"/>.</param>
        /// <returns>The number of bytes written to <paramref name="destination"/>.</returns>
        /// <seealso cref="GetMaxEncodedLength"/>
        int Flush(byte[] destination, int destinationOffset);
    }
}
