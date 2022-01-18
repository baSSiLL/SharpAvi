using SharpAvi.Utilities;

namespace SharpAvi.Format
{
    /// <summary>
    /// Contains definitions of known FOURCC values.
    /// </summary>
    internal static class KnownFourCCs
    {
        /// <summary>
        /// RIFF chunk indentifiers used in AVI format.
        /// </summary>
        internal static class Chunks
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
                CheckStreamIndex(streamIndex);
                return string.Format(compressed ? "{0:00}dc" : "{0:00}db", streamIndex);
            }

            /// <summary>Gets the identifier of an audio data chunk.</summary>
            /// <param name="streamIndex">Sequential number of the stream.</param>
            public static FourCC AudioData(int streamIndex)
            {
                CheckStreamIndex(streamIndex);
                return string.Format("{0:00}wb", streamIndex);
            }

            /// <summary>Gets the identifier of an index chunk.</summary>
            /// <param name="streamIndex">Sequential number of the stream.</param>
            public static FourCC IndexData(int streamIndex)
            {
                CheckStreamIndex(streamIndex);
                return string.Format("ix{0:00}", streamIndex);
            }

            private static void CheckStreamIndex(int streamIndex)
            {
                Argument.IsInRange(streamIndex, 0, 99, nameof(streamIndex));
            }
        }

        /// <summary>
        /// RIFF lists identifiers used in AVI format.
        /// </summary>
        internal static class Lists
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
        internal static class ListTypes
        {
            /// <summary>Top-level list type.</summary>
            public static readonly FourCC Riff = new FourCC("RIFF");

            /// <summary>Non top-level list type.</summary>
            public static readonly FourCC List = new FourCC("LIST");
        }

        /// <summary>
        /// Identifiers of the stream types used in AVI format.
        /// </summary>
        internal static class StreamTypes
        {
            /// <summary>Video stream.</summary>
            public static readonly FourCC Video = new FourCC("vids");

            /// <summary>Audio stream.</summary>
            public static readonly FourCC Audio = new FourCC("auds");
        }


        /// <summary>
        /// Identifiers of codec types used in Video for Windows API.
        /// </summary>
        internal static class CodecTypes
        {
            /// <summary>Video codec.</summary>
            public static readonly FourCC Video = new FourCC("VIDC");
        }
    }
}
