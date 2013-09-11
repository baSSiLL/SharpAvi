using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;

namespace SharpAvi.Output
{
    /// <summary>
    /// Used to write an AVI file.
    /// </summary>
    /// <remarks>
    /// After writing begin to any of the streams, no property changes or stream addition are allowed.
    /// </remarks>
    public class AviWriter : IDisposable, IAviStreamDataHandler
    {
        private const int MAX_SUPER_INDEX_ENTRIES = 256;
        private const int MAX_INDEX_ENTRIES = 15000;
        private const int RIFF_AVI_SIZE_TRESHOLD = 8 * 1024 * 1024;
        private const int RIFF_AVIX_SIZE_TRESHOLD = int.MaxValue - 1024 * 1024;

        private readonly BinaryWriter fileWriter;
        private bool isClosed = false;
        private bool startedWriting = false;
        private readonly object syncWrite = new object();

        private RiffItem currentRiff;
        private RiffItem currentMovie;
        private RiffItem header;
        private int riffSizeTreshold;
        private int riffAviFrameCount = -1;

        private readonly List<IAviStream> streams = new List<IAviStream>();
        private readonly ReadOnlyCollection<IAviStream> streamsRO;
        private StreamInfo[] streamsInfo;

        /// <summary>
        /// Creates a new instance of <see cref="AviWriter"/>.
        /// </summary>
        /// <param name="fileName">Path to an AVI file being written.</param>
        public AviWriter(string fileName)
        {
            streamsRO = streams.AsReadOnly();

            var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None, 1024 * 1024);
            fileWriter = new BinaryWriter(fileStream);
        }

        /// <summary>Frame rate.</summary>
        /// <remarks>
        /// The value of the property is rounded to 3 fractional digits.
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

        /// <summary>AVI streams that have been added so far.</summary>
        public ReadOnlyCollection<IAviStream> Streams
        {
            get { return streamsRO; }
        }

        /// <summary>Adds a new video stream.</summary>
        /// <returns>Newly added video stream.</returns>
        public IAviVideoStream AddVideoStream()
        {
            Contract.Requires(Streams.Count < 100);

            lock (syncWrite)
            {
                CheckNotClosed();
                CheckNotStartedWriting();

                var stream = new AviVideoStream(streams.Count, this);
                streams.Add(stream);
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

                    fileWriter.Close();
                    isClosed = true;
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
            foreach (var stream in streams.Cast<IAviStreamInternal>())
            {
                stream.Freeze();
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
            if (fileWriter.BaseStream.Position + approximateSizeOfNextChunk - currentRiff.ItemStart > riffSizeTreshold)
            {
                CloseCurrentRiff();

                currentRiff = fileWriter.OpenList(KnownFourCCs.Lists.AviExtended, KnownFourCCs.ListTypes.Riff);
                currentMovie = fileWriter.OpenList(KnownFourCCs.Lists.Movie);
            }
        }

        private void CloseCurrentRiff()
        {
            fileWriter.CloseItem(currentMovie);
            fileWriter.CloseItem(currentRiff);

            // Several special actions for the first RIFF (AVI)
            if (currentRiff.ItemStart == 0)
            {
                riffAviFrameCount = streamsInfo.Max(si => si.FrameCount);
                riffSizeTreshold = RIFF_AVIX_SIZE_TRESHOLD;
            }
        }


        void IAviStreamDataHandler.WriteVideoFrame(AviVideoStream stream, bool isKeyFrame, byte[] frameData, int startIndex, int count)
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

                CreateNewRiffIfNeeded(count);

                var chunk = fileWriter.OpenChunk(stream.ChunkId, count);
                fileWriter.Write(frameData, startIndex, count);
                fileWriter.CloseItem(chunk);

                si.OnFrameWritten(chunk.ItemSize);
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
            }
        }


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
            fileWriter.Write((uint)Decimal.Truncate(FramesPerSecond * streamsInfo.Sum(s => s.MaxChunkSize))); // max bytes per second
            fileWriter.Write(0U); // padding granularity
            fileWriter.Write((uint)(MainHeaderFlags.IsInterleaved | MainHeaderFlags.TrustChunkType)); // MainHeaderFlags
            fileWriter.Write(riffAviFrameCount); // total frames (in the first RIFF list containing this header)
            fileWriter.Write(0U); // initial frames
            fileWriter.Write((uint)streams.Count); // stream count
            fileWriter.Write(streamsInfo.Max(s => s.MaxChunkSize)); // suggested buffer size
            fileWriter.Write(streams.OfType<IAviVideoStream>().First().Width); // video width
            fileWriter.Write(streams.OfType<IAviVideoStream>().First().Height); // video height
            fileWriter.SkipBytes(4 * sizeof(uint)); // reserved
            fileWriter.CloseItem(chunk);
        }

        private void WriteOdmlHeader()
        {
            var list = fileWriter.OpenList(KnownFourCCs.Lists.OpenDml);
            var chunk = fileWriter.OpenChunk(KnownFourCCs.Chunks.OpenDmlHeader);
            fileWriter.Write(streamsInfo.Max(s => s.FrameCount)); // total frames in file
            fileWriter.SkipBytes(61 * sizeof(uint)); // reserved
            fileWriter.CloseItem(chunk);
            fileWriter.CloseItem(list);
        }

        private void WriteStreamList(IAviStream stream)
        {
            var list = fileWriter.OpenList(KnownFourCCs.Lists.Stream);
            WriteStreamHeader(stream);
            WriteStreamFormat(stream);
            WriteStreamName(stream);
            WriteStreamSuperIndex(stream);
            fileWriter.CloseItem(list);
        }

        private void WriteStreamHeader(IAviStream stream)
        {
            // See AVISTREAMHEADER structure
            var chunk = fileWriter.OpenChunk(KnownFourCCs.Chunks.StreamHeader);
            fileWriter.Write((uint)((IAviStreamInternal)stream).StreamType);
            fileWriter.Write((uint)stream.Codec);
            fileWriter.Write(0U); // StreamHeaderFlags
            fileWriter.Write((ushort)0); // priority
            fileWriter.Write((ushort)0); // language
            fileWriter.Write(0U); // initial frames
            fileWriter.Write(frameRateDenominator); // scale (frame rate denominator)
            fileWriter.Write(frameRateNumerator); // rate (frame rate numerator)
            fileWriter.Write(0U); // start
            fileWriter.Write((uint)streamsInfo[stream.Index].FrameCount); // length
            fileWriter.Write((uint)streamsInfo[stream.Index].MaxChunkSize); // suggested buffer size
            fileWriter.Write(0U); // quality
            fileWriter.Write(0U); // sample size
            fileWriter.Write((short)0); // rectangle left
            fileWriter.Write((short)0); // rectangle top
            var videoStream = stream as IAviVideoStream;
            short right = (short)(videoStream != null ? videoStream.Width : 0);
            short bottom = (short)(videoStream != null ? videoStream.Height : 0);
            fileWriter.Write(right); // rectangle right
            fileWriter.Write(bottom); // rectangle bottom
            fileWriter.CloseItem(chunk);
        }

        private void WriteStreamFormat(IAviStream stream)
        {
            var chunk = fileWriter.OpenChunk(KnownFourCCs.Chunks.StreamFormat);
            ((IAviStreamInternal)stream).WriteFormat(fileWriter);
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

        private void FlushStreamIndex(IAviStream stream)
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
            fileWriter.Write((uint)((IAviStreamInternal)stream).ChunkId); // chunk ID of the stream
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
