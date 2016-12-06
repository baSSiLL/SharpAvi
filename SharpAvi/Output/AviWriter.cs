using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using SharpAvi.Codecs;

namespace SharpAvi.Output
{
    /// <summary>
    /// Used to write an AVI file.
    /// </summary>
    /// <remarks>
    /// After writing begin to any of the streams, no property changes or stream addition are allowed.
    /// </remarks>
    public class AviWriter : IDisposable, IAviStreamWriteHandler
    {
        private const int MAX_SUPER_INDEX_ENTRIES = 256;
        private const int MAX_INDEX_ENTRIES = 15000;
        private const int INDEX1_ENTRY_SIZE = 4 * sizeof(uint);
        private const int RIFF_AVI_SIZE_TRESHOLD = 512 * 1024 * 1024;
        private const int RIFF_AVIX_SIZE_TRESHOLD = int.MaxValue - 1024 * 1024;

        private readonly BinaryWriter fileWriter;
#if !FX45
        private readonly bool closeWriter;
#endif
        private bool isClosed = false;
        private bool startedWriting = false;
        private readonly object syncWrite = new object();

        private bool isFirstRiff = true;
        private RiffItem currentRiff;
        private RiffItem currentMovie;
        private RiffItem header;
        private int riffSizeTreshold;
        private int riffAviFrameCount = -1;
        private int index1Count = 0;

#if FX45
        private readonly List<IAviStreamInternal> streams = new List<IAviStreamInternal>();
#else
        private readonly List<IAviStream> streamsList = new List<IAviStream>();
        private IEnumerable<IAviStreamInternal> streams
        {
            get { return streamsList.Cast<IAviStreamInternal>(); }
        }
        private readonly ReadOnlyCollection<IAviStream> streamsRO;
#endif
        private StreamInfo[] streamsInfo;

        /// <summary>
        /// Creates a new instance of <see cref="AviWriter"/> for writing to a file.
        /// </summary>
        /// <param name="fileName">Path to an AVI file being written.</param>
        public AviWriter(string fileName)
        {
            Contract.Requires(!string.IsNullOrEmpty(fileName));

#if !FX45
            streamsRO = new ReadOnlyCollection<IAviStream>(streamsList);
#endif

            var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None, 1024 * 1024);
            fileWriter = new BinaryWriter(fileStream);
        }

        /// <summary>
        /// Creates a new instance of <see cref="AviWriter"/> for writing to a stream.
        /// </summary>
        /// <param name="stream">Stream being written to.</param>
        /// <param name="leaveOpen">Whether to leave the stream open when closing <see cref="AviWriter"/>.</param>
        public AviWriter(Stream stream, bool leaveOpen = false)
        {
            Contract.Requires(stream.CanWrite);
            Contract.Requires(stream.CanSeek);

#if FX45
            fileWriter = new BinaryWriter(stream, Encoding.Default, leaveOpen);
#else
            fileWriter = new BinaryWriter(stream);
            closeWriter = !leaveOpen;
            streamsRO = new ReadOnlyCollection<IAviStream>(streamsList);
#endif

        }

        /// <summary>Frame rate.</summary>
        /// <remarks>
        /// The value of the property is rounded to 3 fractional digits.
        /// Default value is <c>1</c>.
        /// </remarks>
        public decimal FramesPerSecond
        {
            get { return framesPerSecond; }
            set
            {
                Contract.Requires(value > 0);

                lock (syncWrite)
                {
                    CheckNotStartedWriting();
                    framesPerSecond = Decimal.Round(value, 3);
                }
            }
        }
        private decimal framesPerSecond = 1;
        private uint frameRateNumerator;
        private uint frameRateDenominator;

        /// <summary>
        /// Whether to emit index used in AVI v1 format.
        /// </summary>
        /// <remarks>
        /// By default, only index conformant to OpenDML AVI extensions (AVI v2) is emitted. 
        /// Presence of v1 index may improve the compatibility of generated AVI files with certain software, 
        /// especially when there are multiple streams.
        /// </remarks>
        public bool EmitIndex1
        {
            get { return emitIndex1; }
            set
            {
                lock (syncWrite)
                {
                    CheckNotStartedWriting();
                    emitIndex1 = value;
                }
            }
        }
        private bool emitIndex1;

        /// <summary>AVI streams that have been added so far.</summary>
#if FX45
        public IReadOnlyList<IAviStream> Streams
        {
            get { return streams; }
        }
#else
        public ReadOnlyCollection<IAviStream> Streams
        {
            get { return streamsRO; }
        }
#endif

        /// <summary>Adds new video stream.</summary>
        /// <param name="width">Frame's width.</param>
        /// <param name="height">Frame's height.</param>
        /// <param name="bitsPerPixel">Bits per pixel.</param>
        /// <returns>Newly added video stream.</returns>
        /// <remarks>
        /// Stream is initialized to be ready for uncompressed video (bottom-up BGR) with specified parameters.
        /// However, properties (such as <see cref="IAviVideoStream.Codec"/>) can be changed later if the stream is
        /// to be fed with pre-compressed data.
        /// </remarks>
        public IAviVideoStream AddVideoStream(int width = 1, int height = 1, BitsPerPixel bitsPerPixel = BitsPerPixel.Bpp32)
        {
            Contract.Requires(width > 0);
            Contract.Requires(height > 0);
            Contract.Requires(Enum.IsDefined(typeof(BitsPerPixel), bitsPerPixel));
            Contract.Requires(Streams.Count < 100);
            Contract.Ensures(Contract.Result<IAviVideoStream>() != null);

            return AddStream<IAviVideoStreamInternal>(index => 
                {
                    var stream = new AviVideoStream(index, this, width, height, bitsPerPixel);
                    var asyncStream = new AsyncVideoStreamWrapper(stream);
                    return asyncStream;
                });
        }

        /// <summary>Adds new encoding video stream.</summary>
        /// <param name="encoder">Encoder to be used.</param>
        /// <param name="ownsEncoder">Whether encoder should be disposed with the writer.</param>
        /// <param name="width">Frame's width.</param>
        /// <param name="height">Frame's height.</param>
        /// <returns>Newly added video stream.</returns>
        /// <remarks>
        /// <para>
        /// Stream is initialized to be to be encoded with the specified encoder.
        /// Method <see cref="IAviVideoStream.WriteFrame"/> expects data in the same format as encoders,
        /// that is top-down BGR32 bitmap. It is passed to the encoder and the encoded result is written
        /// to the stream.
        /// Parameters <c>isKeyFrame</c> and <c>length</c> are ignored by encoding streams,
        /// as encoders determine on their own which frames are keys, and the size of input bitmaps is fixed.
        /// </para>
        /// <para>
        /// Properties <see cref="IAviVideoStream.Codec"/> and <see cref="IAviVideoStream.BitsPerPixel"/> 
        /// are defined by the encoder, and cannot be modified.
        /// </para>
        /// </remarks>
        public IAviVideoStream AddEncodingVideoStream(IVideoEncoder encoder, bool ownsEncoder = true, int width = 1, int height = 1)
        {
            Contract.Requires(encoder != null);
            Contract.Requires(Streams.Count < 100);
            Contract.Ensures(Contract.Result<IAviVideoStream>() != null);

            return AddStream<IAviVideoStreamInternal>(index => 
                {
                    var stream = new AviVideoStream(index, this, width, height, BitsPerPixel.Bpp32);
                    var encodingStream = new EncodingVideoStreamWrapper(stream, encoder, ownsEncoder);
                    var asyncStream = new AsyncVideoStreamWrapper(encodingStream);
                    return asyncStream;
                });
        }

        /// <summary>Adds new audio stream.</summary>
        /// <param name="channelCount">Number of channels.</param>
        /// <param name="samplesPerSecond">Sample rate.</param>
        /// <param name="bitsPerSample">Bits per sample (per single channel).</param>
        /// <returns>Newly added audio stream.</returns>
        /// <remarks>
        /// Stream is initialized to be ready for uncompressed audio (PCM) with specified parameters.
        /// However, properties (such as <see cref="IAviAudioStream.Format"/>) can be changed later if the stream is
        /// to be fed with pre-compressed data.
        /// </remarks>
        public IAviAudioStream AddAudioStream(int channelCount = 1, int samplesPerSecond = 44100, int bitsPerSample = 16)
        {
            Contract.Requires(channelCount > 0);
            Contract.Requires(samplesPerSecond > 0);
            Contract.Requires(bitsPerSample > 0 && (bitsPerSample % 8) == 0);
            Contract.Requires(Streams.Count < 100);
            Contract.Ensures(Contract.Result<IAviAudioStream>() != null);

            return AddStream<IAviAudioStreamInternal>(index => 
                {
                    var stream = new AviAudioStream(index, this, channelCount, samplesPerSecond, bitsPerSample);
                    var asyncStream = new AsyncAudioStreamWrapper(stream);
                    return asyncStream;
                });
        }

        /// <summary>Adds new encoding audio stream.</summary>
        /// <param name="encoder">Encoder to be used.</param>
        /// <param name="ownsEncoder">Whether encoder should be disposed with the writer.</param>
        /// <returns>Newly added audio stream.</returns>
        /// <remarks>
        /// <para>
        /// Stream is initialized to be to be encoded with the specified encoder.
        /// Method <see cref="IAviAudioStream.WriteBlock"/> expects data in the same format as encoder (see encoder's docs). 
        /// The data is passed to the encoder and the encoded result is written to the stream.
        /// </para>
        /// <para>
        /// The encoder defines the following properties of the stream:
        /// <see cref="IAviAudioStream.ChannelCount"/>, <see cref="IAviAudioStream.SamplesPerSecond"/>,
        /// <see cref="IAviAudioStream.BitsPerSample"/>, <see cref="IAviAudioStream.BytesPerSecond"/>,
        /// <see cref="IAviAudioStream.Granularity"/>, <see cref="IAviAudioStream.Format"/>,
        /// <see cref="IAviAudioStream.FormatSpecificData"/>.
        /// These properties cannot be modified.
        /// </para>
        /// </remarks>
        public IAviAudioStream AddEncodingAudioStream(IAudioEncoder encoder, bool ownsEncoder = true)
        {
            Contract.Requires(encoder != null);
            Contract.Requires(Streams.Count < 100);
            Contract.Ensures(Contract.Result<IAviAudioStream>() != null);

            return AddStream<IAviAudioStreamInternal>(index => 
                {
                    var stream = new AviAudioStream(index, this, 1, 44100, 16);
                    var encodingStream = new EncodingAudioStreamWrapper(stream, encoder, ownsEncoder);
                    var asyncStream = new AsyncAudioStreamWrapper(encodingStream);
                    return asyncStream;
                });
        }

        private TStream AddStream<TStream>(Func<int, TStream> streamFactory)
            where TStream : IAviStreamInternal
        {
            Contract.Requires(streamFactory != null);
            Contract.Requires(Streams.Count < 100);

            lock (syncWrite)
            {
                CheckNotClosed();
                CheckNotStartedWriting();

                var stream = streamFactory.Invoke(Streams.Count);
#if FX45
                streams.Add(stream);
#else
                streamsList.Add(stream);
#endif
                return stream;
            }
        }

        /// <summary>
        /// Closes the writer and AVI file itself.
        /// </summary>
        public void Close()
        {
            if (!isClosed)
            {
                bool finishWriting;
                lock (syncWrite)
                {
                    finishWriting = startedWriting;
                }
                // Call FinishWriting without holding the lock
                // because additional writes may be performed inside
                if (finishWriting)
                {
                    foreach (var stream in streams)
                    {
                        stream.FinishWriting();
                    }
                }

                lock (syncWrite)
                {
                    if (startedWriting)
                    {
                        foreach (var stream in streams)
                        {
                            FlushStreamIndex(stream);
                        }

                        CloseCurrentRiff();

                        // Rewrite header with actual data like frames count, super index, etc.
                        fileWriter.BaseStream.Position = header.ItemStart;
                        WriteHeader();
                    }

#if FX45
                    fileWriter.Close();
#else
                    if (closeWriter)
                    {
                        fileWriter.Close();
                    }
                    else
                    {
                        fileWriter.Flush();
                    }
#endif
                    isClosed = true;
                }

                foreach (var disposableStream in streams.OfType<IDisposable>())
                {
                    disposableStream.Dispose();
                }
            }
        }

        void IDisposable.Dispose()
        {
            Close();
        }

        private void CheckNotStartedWriting()
        {
            if (startedWriting)
            {
                throw new InvalidOperationException("No stream information can be changed after starting to write frames.");
            }
        }

        private void CheckNotClosed()
        {
            if (isClosed)
            {
                throw new ObjectDisposedException(typeof(AviWriter).Name);
            }
        }

        private void PrepareForWriting()
        {
            startedWriting = true;
            foreach (var stream in streams)
            {
                stream.PrepareForWriting();
            }
            AviUtils.SplitFrameRate(FramesPerSecond, out frameRateNumerator, out frameRateDenominator);

            streamsInfo = streams.Select(s => new StreamInfo(KnownFourCCs.Chunks.IndexData(s.Index))).ToArray();

            riffSizeTreshold = RIFF_AVI_SIZE_TRESHOLD;

            currentRiff = fileWriter.OpenList(KnownFourCCs.Lists.Avi, KnownFourCCs.ListTypes.Riff);
            WriteHeader();
            currentMovie = fileWriter.OpenList(KnownFourCCs.Lists.Movie);
        }

        private void CreateNewRiffIfNeeded(int approximateSizeOfNextChunk)
        {
            var estimatedSize = fileWriter.BaseStream.Position + approximateSizeOfNextChunk - currentRiff.ItemStart;
            if (isFirstRiff && emitIndex1)
            {
                estimatedSize += RiffItem.ITEM_HEADER_SIZE + index1Count * INDEX1_ENTRY_SIZE;
            }
            if (estimatedSize > riffSizeTreshold)
            {
                CloseCurrentRiff();

                currentRiff = fileWriter.OpenList(KnownFourCCs.Lists.AviExtended, KnownFourCCs.ListTypes.Riff);
                currentMovie = fileWriter.OpenList(KnownFourCCs.Lists.Movie);
            }
        }

        private void CloseCurrentRiff()
        {
            fileWriter.CloseItem(currentMovie);

            // Several special actions for the first RIFF (AVI)
            if (isFirstRiff)
            {
                riffAviFrameCount = streams.OfType<IAviVideoStream>().Max(s => streamsInfo[s.Index].FrameCount);
                if (emitIndex1)
                {
                    WriteIndex1();
                }
                riffSizeTreshold = RIFF_AVIX_SIZE_TRESHOLD;
            }

            fileWriter.CloseItem(currentRiff);
            isFirstRiff = false;
        }


        #region IAviStreamDataHandler implementation

        void IAviStreamWriteHandler.WriteVideoFrame(AviVideoStream stream, bool isKeyFrame, byte[] frameData, int startIndex, int count)
        {
            WriteStreamFrame(stream, isKeyFrame, frameData, startIndex, count);
        }

        void IAviStreamWriteHandler.WriteAudioBlock(AviAudioStream stream, byte[] blockData, int startIndex, int count)
        {
            WriteStreamFrame(stream, true, blockData, startIndex, count);
        }

        private void WriteStreamFrame(AviStreamBase stream, bool isKeyFrame, byte[] frameData, int startIndex, int count)
        {
            lock (syncWrite)
            {
                CheckNotClosed();

                if (!startedWriting)
                {
                    PrepareForWriting();
                }

                var si = streamsInfo[stream.Index];
                if (si.SuperIndex.Count == MAX_SUPER_INDEX_ENTRIES)
                {
                    throw new InvalidOperationException("Cannot write more frames to this stream.");
                }

                if (ShouldFlushStreamIndex(si.StandardIndex))
                {
                    FlushStreamIndex(stream);
                }

                var shouldCreateIndex1Entry = emitIndex1 && isFirstRiff;

                CreateNewRiffIfNeeded(count + (shouldCreateIndex1Entry ? INDEX1_ENTRY_SIZE : 0));

                var chunk = fileWriter.OpenChunk(stream.ChunkId, count);
                fileWriter.Write(frameData, startIndex, count);
                fileWriter.CloseItem(chunk);

                si.OnFrameWritten(chunk.DataSize);
                var dataSize = (uint)chunk.DataSize;
                // Set highest bit for non-key frames according to the OpenDML spec
                if (!isKeyFrame)
                {
                    dataSize |= 0x80000000U;
                }

                var newEntry = new StandardIndexEntry
                {
                    DataOffset = chunk.DataStart,
                    DataSize = dataSize
                };
                si.StandardIndex.Add(newEntry);

                if (shouldCreateIndex1Entry)
                {
                    var index1Entry = new Index1Entry
                    {
                        IsKeyFrame = isKeyFrame,
                        DataOffset = (uint)(chunk.ItemStart - currentMovie.DataStart),
                        DataSize = dataSize
                    };
                    si.Index1.Add(index1Entry);
                    index1Count++;
                }
            }
        }

        void IAviStreamWriteHandler.WriteStreamHeader(AviVideoStream videoStream)
        {
            // See AVISTREAMHEADER structure
            fileWriter.Write((uint)videoStream.StreamType);
            fileWriter.Write((uint)videoStream.Codec);
            fileWriter.Write(0U); // StreamHeaderFlags
            fileWriter.Write((ushort)0); // priority
            fileWriter.Write((ushort)0); // language
            fileWriter.Write(0U); // initial frames
            fileWriter.Write(frameRateDenominator); // scale (frame rate denominator)
            fileWriter.Write(frameRateNumerator); // rate (frame rate numerator)
            fileWriter.Write(0U); // start
            fileWriter.Write((uint)streamsInfo[videoStream.Index].FrameCount); // length
            fileWriter.Write((uint)streamsInfo[videoStream.Index].MaxChunkDataSize); // suggested buffer size
            fileWriter.Write(0U); // quality
            fileWriter.Write(0U); // sample size
            fileWriter.Write((short)0); // rectangle left
            fileWriter.Write((short)0); // rectangle top
            short right = (short)(videoStream != null ? videoStream.Width : 0);
            short bottom = (short)(videoStream != null ? videoStream.Height : 0);
            fileWriter.Write(right); // rectangle right
            fileWriter.Write(bottom); // rectangle bottom
        }

        void IAviStreamWriteHandler.WriteStreamHeader(AviAudioStream audioStream)
        {
            // See AVISTREAMHEADER structure
            fileWriter.Write((uint)audioStream.StreamType);
            fileWriter.Write(0U); // no codec
            fileWriter.Write(0U); // StreamHeaderFlags
            fileWriter.Write((ushort)0); // priority
            fileWriter.Write((ushort)0); // language
            fileWriter.Write(0U); // initial frames
            fileWriter.Write((uint)audioStream.Granularity); // scale (sample rate denominator)
            fileWriter.Write((uint)audioStream.BytesPerSecond); // rate (sample rate numerator)
            fileWriter.Write(0U); // start
            fileWriter.Write((uint)streamsInfo[audioStream.Index].TotalDataSize); // length
            fileWriter.Write((uint)(audioStream.BytesPerSecond / 2)); // suggested buffer size (half-second)
            fileWriter.Write(-1); // quality
            fileWriter.Write(audioStream.Granularity); // sample size
            fileWriter.SkipBytes(sizeof(short) * 4);
        }

        void IAviStreamWriteHandler.WriteStreamFormat(AviVideoStream videoStream)
        {
            // See BITMAPINFOHEADER structure
            fileWriter.Write(40U); // size of structure
            fileWriter.Write(videoStream.Width);
            fileWriter.Write(videoStream.Height);
            fileWriter.Write((short)1); // planes
            fileWriter.Write((ushort)videoStream.BitsPerPixel); // bits per pixel
            fileWriter.Write((uint)videoStream.Codec); // compression (codec FOURCC)
            var sizeInBytes = videoStream.Width * videoStream.Height * (((int)videoStream.BitsPerPixel) / 8);
            fileWriter.Write((uint)sizeInBytes); // image size in bytes
            fileWriter.Write(0); // X pixels per meter
            fileWriter.Write(0); // Y pixels per meter

            // Writing grayscale palette for 8-bit uncompressed stream
            // Otherwise, no palette
            if (videoStream.BitsPerPixel == BitsPerPixel.Bpp8 && videoStream.Codec == KnownFourCCs.Codecs.Uncompressed)
            {
                fileWriter.Write(256U); // palette colors used
                fileWriter.Write(0U); // palette colors important
                for (int i = 0; i < 256; i++)
                {
                    fileWriter.Write((byte)i);
                    fileWriter.Write((byte)i);
                    fileWriter.Write((byte)i);
                    fileWriter.Write((byte)0);
                }
            }
            else
            {
                fileWriter.Write(0U); // palette colors used
                fileWriter.Write(0U); // palette colors important
            }
        }

        void IAviStreamWriteHandler.WriteStreamFormat(AviAudioStream audioStream)
        {
            // See WAVEFORMATEX structure
            fileWriter.Write(audioStream.Format);
            fileWriter.Write((ushort)audioStream.ChannelCount);
            fileWriter.Write((uint)audioStream.SamplesPerSecond);
            fileWriter.Write((uint)audioStream.BytesPerSecond);
            fileWriter.Write((ushort)audioStream.Granularity);
            fileWriter.Write((ushort)audioStream.BitsPerSample);
            if (audioStream.FormatSpecificData != null)
            {
                fileWriter.Write((ushort)audioStream.FormatSpecificData.Length);
                fileWriter.Write(audioStream.FormatSpecificData);
            }
            else
            {
                fileWriter.Write((ushort)0);
            }
        }

        #endregion


        #region Header

        private void WriteHeader()
        {
            header = fileWriter.OpenList(KnownFourCCs.Lists.Header);
            WriteFileHeader();
            foreach (var stream in streams)
            {
                WriteStreamList(stream);
            }
            WriteOdmlHeader();
            WriteJunkInsteadOfMissingSuperIndexEntries();
            fileWriter.CloseItem(header);
        }

        private void WriteJunkInsteadOfMissingSuperIndexEntries()
        {
            var missingEntriesCount = streamsInfo.Sum(si => MAX_SUPER_INDEX_ENTRIES - si.SuperIndex.Count);
            if (missingEntriesCount > 0)
            {
                var junkDataSize = missingEntriesCount * sizeof(uint) * 4 - RiffItem.ITEM_HEADER_SIZE;
                var chunk = fileWriter.OpenChunk(KnownFourCCs.Chunks.Junk, junkDataSize);
                fileWriter.SkipBytes(junkDataSize);
                fileWriter.CloseItem(chunk);
            }
        }

        private void WriteFileHeader()
        {
            // See AVIMAINHEADER structure
            var chunk = fileWriter.OpenChunk(KnownFourCCs.Chunks.AviHeader);
            fileWriter.Write((uint)Decimal.Round(1000000m / FramesPerSecond)); // microseconds per frame
            // TODO: More correct computation of byterate
            fileWriter.Write((uint)Decimal.Truncate(FramesPerSecond * streamsInfo.Sum(s => s.MaxChunkDataSize))); // max bytes per second
            fileWriter.Write(0U); // padding granularity
            var flags = MainHeaderFlags.IsInterleaved | MainHeaderFlags.TrustChunkType;
            if (emitIndex1)
            {
                flags |= MainHeaderFlags.HasIndex;
            }
            fileWriter.Write((uint)flags); // MainHeaderFlags
            fileWriter.Write(riffAviFrameCount); // total frames (in the first RIFF list containing this header)
            fileWriter.Write(0U); // initial frames
            fileWriter.Write((uint)Streams.Count); // stream count
            fileWriter.Write(0U); // suggested buffer size
            var firstVideoStream = streams.OfType<IAviVideoStream>().First();
            fileWriter.Write(firstVideoStream.Width); // video width
            fileWriter.Write(firstVideoStream.Height); // video height
            fileWriter.SkipBytes(4 * sizeof(uint)); // reserved
            fileWriter.CloseItem(chunk);
        }

        private void WriteOdmlHeader()
        {
            var list = fileWriter.OpenList(KnownFourCCs.Lists.OpenDml);
            var chunk = fileWriter.OpenChunk(KnownFourCCs.Chunks.OpenDmlHeader);
            fileWriter.Write(streams.OfType<IAviVideoStream>().Max(s => streamsInfo[s.Index].FrameCount)); // total frames in file
            fileWriter.SkipBytes(61 * sizeof(uint)); // reserved
            fileWriter.CloseItem(chunk);
            fileWriter.CloseItem(list);
        }

        private void WriteStreamList(IAviStreamInternal stream)
        {
            var list = fileWriter.OpenList(KnownFourCCs.Lists.Stream);
            WriteStreamHeader(stream);
            WriteStreamFormat(stream);
            WriteStreamName(stream);
            WriteStreamSuperIndex(stream);
            fileWriter.CloseItem(list);
        }

        private void WriteStreamHeader(IAviStreamInternal stream)
        {
            var chunk = fileWriter.OpenChunk(KnownFourCCs.Chunks.StreamHeader);
            stream.WriteHeader();
            fileWriter.CloseItem(chunk);
        }

        private void WriteStreamFormat(IAviStreamInternal stream)
        {
            var chunk = fileWriter.OpenChunk(KnownFourCCs.Chunks.StreamFormat);
            stream.WriteFormat();
            fileWriter.CloseItem(chunk);
        }

        private void WriteStreamName(IAviStream stream)
        {
            if (!string.IsNullOrEmpty(stream.Name))
            {
                var bytes = Encoding.ASCII.GetBytes(stream.Name);
                var chunk = fileWriter.OpenChunk(KnownFourCCs.Chunks.StreamName);
                fileWriter.Write(bytes);
                fileWriter.Write((byte)0);
                fileWriter.CloseItem(chunk);
            }
        }

        private void WriteStreamSuperIndex(IAviStream stream)
        {
            var superIndex = streamsInfo[stream.Index].SuperIndex;

            // See AVISUPERINDEX structure
            var chunk = fileWriter.OpenChunk(KnownFourCCs.Chunks.StreamIndex);
            fileWriter.Write((ushort)4); // DWORDs per entry
            fileWriter.Write((byte)0); // index sub-type
            fileWriter.Write((byte)IndexType.Indexes); // index type
            fileWriter.Write((uint)superIndex.Count); // entries count
            fileWriter.Write((uint)((IAviStreamInternal)stream).ChunkId); // chunk ID of the stream
            fileWriter.SkipBytes(3 * sizeof(uint)); // reserved
            
            // entries
            foreach (var entry in superIndex)
            {
                fileWriter.Write((ulong)entry.ChunkOffset); // offset of sub-index chunk
                fileWriter.Write((uint)entry.ChunkSize); // size of sub-index chunk
                fileWriter.Write((uint)entry.Duration); // duration of sub-index data (number of frames it refers to)
            }

            fileWriter.CloseItem(chunk);
        }

        #endregion


        #region Index

        private void WriteIndex1()
        {
            var chunk = fileWriter.OpenChunk(KnownFourCCs.Chunks.Index1);

            var indices = streamsInfo.Select((si, i) => new {si.Index1, ChunkId = (uint)streams.ElementAt(i).ChunkId}).
                Where(a => a.Index1.Count > 0)
                .ToList();
            while (index1Count > 0)
            {
                var minOffset = indices[0].Index1[0].DataOffset;
                var minIndex = 0;
                for (var i = 1; i < indices.Count; i++)
                {
                    var offset = indices[i].Index1[0].DataOffset;
                    if (offset < minOffset)
                    {
                        minOffset = offset;
                        minIndex = i;
                    }
                }

                var index = indices[minIndex];
                fileWriter.Write(index.ChunkId);
                fileWriter.Write(index.Index1[0].IsKeyFrame ? 0x00000010U : 0);
                fileWriter.Write(index.Index1[0].DataOffset);
                fileWriter.Write(index.Index1[0].DataSize);

                index.Index1.RemoveAt(0);
                if (index.Index1.Count == 0)
                {
                    indices.RemoveAt(minIndex);
                }

                index1Count--;
            }

            fileWriter.CloseItem(chunk);
        }

        private bool ShouldFlushStreamIndex(IList<StandardIndexEntry> index)
        {
            // Check maximum number of entries
            if (index.Count >= MAX_INDEX_ENTRIES)
                return true;

            // Check relative offset
            if (index.Count > 0 && fileWriter.BaseStream.Position - index[0].DataOffset > uint.MaxValue)
                return true;

            return false;
        }

        private void FlushStreamIndex(IAviStreamInternal stream)
        {
            var si = streamsInfo[stream.Index];
            var index = si.StandardIndex;
            var entriesCount = index.Count;
            if (entriesCount == 0)
                return;

            var baseOffset = index[0].DataOffset;
            var indexSize = 24 + entriesCount * 8;

            CreateNewRiffIfNeeded(indexSize);

            // See AVISTDINDEX structure
            var chunk = fileWriter.OpenChunk(si.StandardIndexChunkId, indexSize);
            fileWriter.Write((ushort)2); // DWORDs per entry
            fileWriter.Write((byte)0); // index sub-type
            fileWriter.Write((byte)IndexType.Chunks); // index type
            fileWriter.Write((uint)entriesCount); // entries count
            fileWriter.Write((uint)stream.ChunkId); // chunk ID of the stream
            fileWriter.Write((ulong)baseOffset); // base offset for entries
            fileWriter.SkipBytes(sizeof(uint)); // reserved

            foreach (var entry in index)
            {
                fileWriter.Write((uint)(entry.DataOffset - baseOffset)); // chunk data offset
                fileWriter.Write(entry.DataSize); // chunk data size
            }

            fileWriter.CloseItem(chunk);

            var superIndex = streamsInfo[stream.Index].SuperIndex;
            var newEntry = new SuperIndexEntry
            {
                ChunkOffset = chunk.ItemStart,
                ChunkSize = chunk.ItemSize,
                Duration = entriesCount
            };
            superIndex.Add(newEntry);

            index.Clear();
        }

        #endregion
    }
}
