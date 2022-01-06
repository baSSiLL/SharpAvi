namespace SharpAvi.Output
{
    /// <summary>
    /// A stream of AVI files.
    /// </summary>
    public interface IAviStream
    {
        /// <summary>
        /// Serial number of this stream in AVI file.
        /// </summary>
        int Index { get; }

        /// <summary>Name of the stream.</summary>
        /// <remarks>May be used by some players when displaying the list of available streams.</remarks>
        string Name { get; set; }
    }
}
