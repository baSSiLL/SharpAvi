using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAvi.Codecs
{
    partial class LameMp3AudioEncoder
    {
        /// <summary>
        /// Interface is used to access the API of the LAME DLL.
        /// </summary>
        /// <remarks>
        /// Clients of <see cref="LameMp3AudioEncoder"/> class need not to work with
        /// this interface directly.
        /// </remarks>
        public interface ILameFacade
        {
            int ChannelCount { get; set; }
            int InputSampleRate { get; set; }
            int OutputBitRate { get; set; }
            int OutputSampleRate { get; }
            int FrameSize { get; }
            int EncoderDelay { get; }

            void PrepareEncoding();
            int Encode(byte[] source, int sourceIndex, int sampleCount, byte[] dest, int destIndex);
        }
    }
}
