using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace SharpAvi
{
    /// <summary>
    /// Contains definitions of known FOURCC values.
    /// </summary>
    public static class KnownFourCCs
    {
        /// <summary>
        /// RIFF chunk indentifiers used in AVI format.
        /// </summary>
        public static class Chunks
        {
            /// <summary>Main AVI header.</summary>
            public static readonly FourCC AviHeader = new FourCC("avih");

            /// <summary>Stream header.</summary>
            public static readonly FourCC StreamHeader = new FourCC("strh");

            /// <summary>Stream format.</summary>
            public static readonly FourCC StreamFormat = new FourCC("strf");

            /// <summary>Stream name.</summary>
            public static readonly FourCC StreamName = new FourCC("strn");

            /// <summary>Stream index.</summary>
            public static readonly FourCC StreamIndex = new FourCC("indx");

            /// <summary>Index v1.</summary>
            public static readonly FourCC Index1 = new FourCC("idx1");

            /// <summary>OpenDML header.</summary>
            public static readonly FourCC OpenDmlHeader = new FourCC("dmlh");

            /// <summary>Junk chunk.</summary>
            public static readonly FourCC Junk = new FourCC("JUNK");

            /// <summary>Gets the identifier of a video frame chunk.</summary>
            /// <param name="streamIndex">Sequential number of the stream.</param>
            /// <param name="compressed">Whether stream contents is compressed.</param>
            public static FourCC VideoFrame(int streamIndex, bool compressed)
            {
                Contract.Requires(0 <= streamIndex && streamIndex <= 99);

                return string.Format(compressed ? "{0:00}dc" : "{0:00}db", streamIndex);
            }

            /// <summary>Gets the identifier of an audio data chunk.</summary>
            /// <param name="streamIndex">Sequential number of the stream.</param>
            public static FourCC AudioData(int streamIndex)
            {
                Contract.Requires(0 <= streamIndex && streamIndex <= 99);

                return string.Format("{0:00}wb", streamIndex);
            }

            /// <summary>Gets the identifier of an index chunk.</summary>
            /// <param name="streamIndex">Sequential number of the stream.</param>
            public static FourCC IndexData(int streamIndex)
            {
                Contract.Requires(0 <= streamIndex && streamIndex <= 99);

                return string.Format("ix{0:00}", streamIndex);
            }
        }

        /// <summary>
        /// RIFF lists identifiers used in AVI format.
        /// </summary>
        public static class Lists
        {
            /// <summary>Top-level AVI list.</summary>
            public static readonly FourCC Avi = new FourCC("AVI");

            /// <summary>Top-level extended AVI list.</summary>
            public static readonly FourCC AviExtended = new FourCC("AVIX");

            /// <summary>Header list.</summary>
            public static readonly FourCC Header = new FourCC("hdrl");

            /// <summary>List containing stream information.</summary>
            public static readonly FourCC Stream = new FourCC("strl");

            /// <summary>List containing OpenDML headers.</summary>
            public static readonly FourCC OpenDml = new FourCC("odml");

            /// <summary>List with content chunks.</summary>
            public static readonly FourCC Movie = new FourCC("movi");
        }

        /// <summary>
        /// Identifiers of the list types used in RIFF format.
        /// </summary>
        public static class ListTypes
        {
            /// <summary>Top-level list type.</summary>
            public static readonly FourCC Riff = new FourCC("RIFF");

            /// <summary>Non top-level list type.</summary>
            public static readonly FourCC List = new FourCC("LIST");
        }

        /// <summary>
        /// Identifiers of the stream types used in AVI format.
        /// </summary>
        public static class StreamTypes
        {
            /// <summary>Video stream.</summary>
            public static readonly FourCC Video = new FourCC("vids");

            /// <summary>Audio stream.</summary>
            public static readonly FourCC Audio = new FourCC("auds");
        }

        /// <summary>Identifiers of various codecs.</summary>
        public static class Codecs
        {
            /// <summary>Identifier used for non-compressed data.</summary>
            public static readonly FourCC Uncompressed = new FourCC(0);

            /// <summary>Motion JPEG.</summary>
            public static readonly FourCC MotionJpeg = new FourCC("MJPG");

            /// <summary>Microsoft MPEG-4 V3.</summary>
            public static readonly FourCC MicrosoftMpeg4V3 = new FourCC("MP43");

            /// <summary>Microsoft MPEG-4 V2.</summary>
            public static readonly FourCC MicrosoftMpeg4V2 = new FourCC("MP42");

            /// <summary>Xvid MPEG-4.</summary>
            public static readonly FourCC Xvid = new FourCC("XVID");

            /// <summary>DivX MPEG-4.</summary>
            public static readonly FourCC DivX = new FourCC("DIVX");

            /// <summary>x264 H.264/MPEG-4 AVC.</summary>
            public static readonly FourCC X264 = new FourCC("X264");
        }

        /// <summary>
        /// Identifiers of codec types used in Video for Windows API.
        /// </summary>
        public static class CodecTypes
        {
            /// <summary>Video codec.</summary>
            public static readonly FourCC Video = new FourCC("VIDC");
        }
    }
}
