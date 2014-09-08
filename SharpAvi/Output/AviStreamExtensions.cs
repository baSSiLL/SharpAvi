using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpAvi.Codecs;
using System.Diagnostics.Contracts;

namespace SharpAvi.Output
{
    /// <summary>
    /// Extension methods for the convenient creation of wrappers for AVI streams.
    /// </summary>
    public static class AviStreamExtensions
    {
        /// <summary>Creates the encoding wrapper for an AVI stream.</summary>
        /// <param name="stream">Stream to be wrapped.</param>
        /// <param name="encoder">Encoder to be used.</param>
        public static EncodingVideoStreamWrapper WithEncoder(this IAviVideoStream stream, IVideoEncoder encoder)
        {
            Contract.Requires(stream != null);
            Contract.Requires(encoder != null);

            return new EncodingVideoStreamWrapper(stream, encoder);
        }

        /// <summary>Creates the encoding wrapper for an AVI stream.</summary>
        /// <param name="stream">Stream to be wrapped.</param>
        /// <param name="encoder">Encoder to be used.</param>
        public static EncodingAudioStreamWrapper WithEncoder(this IAviAudioStream stream, IAudioEncoder encoder)
        {
            Contract.Requires(stream != null);
            Contract.Requires(encoder != null);

            return new EncodingAudioStreamWrapper(stream, encoder);
        }

        /// <summary>Creates the asynchronous wrapper for an AVI stream.</summary>
        /// <param name="stream">Stream to be wrapped.</param>
        public static AsyncVideoStreamWrapper Async(this IAviVideoStream stream)
        {
            Contract.Requires(stream != null);

            return new AsyncVideoStreamWrapper(stream);
        }
    }
}
