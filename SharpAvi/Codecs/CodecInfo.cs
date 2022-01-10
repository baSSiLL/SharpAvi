namespace SharpAvi.Codecs
{
    /// <summary>
    /// Information about a codec.
    /// </summary>
    public class CodecInfo
    {

        /// <summary>
        /// Creates a new instance of <see cref="CodecInfo"/>.
        /// </summary>
        public CodecInfo(FourCC codec, string name)
        {
            this.Codec = codec;
            this.Name = name;
        }

        /// <summary>Codec ID.</summary>
        public FourCC Codec { get; }

        /// <summary>
        /// Descriptive codec name that may be show to a user.
        /// </summary>
        public string Name { get; }
    }
}
