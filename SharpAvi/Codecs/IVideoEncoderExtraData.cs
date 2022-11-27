using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAvi.Codecs
{
    /// <summary>
    /// Export extra data for some codecs
    /// </summary>
    public interface IVideoEncoderExtraData
    {
        /// <summary>
        /// Encoded images header with extra data
        /// </summary>
        byte[] BitmapInfoHeader { get; }
    }
}
