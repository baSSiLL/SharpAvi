namespace SharpAvi.Codecs
{
    /// <summary>
    /// Information about a codec.
    /// </summary>
    public class CodecInfo
    {
        private readonly FourCC codec;
        private readonly string name;

        /// <summary>
        /// Creates a new instance of <see cref="CodecInfo"/>.
        /// </summary>
        public CodecInfo(FourCC codec, string name)
        {
            this.codec = codec;
            this.name = name;
        }

        /// <summary>Codec ID.</summary>
        public FourCC Codec
        {
            get { return codec; }
        }

        /// <summary>
        /// Descriptive codec name that may be show to a user.
        /// </summary>
        public string Name
        {
            get { return name; }
        }
    }
}
