using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace SharpAvi.Codecs
{
    internal class BitmapUtils
    {
        public static unsafe void Bgr32ToBgr24(byte[] source, int srcOffset, byte[] destination, int destOffset, int pixelCount)
        {
            Contract.Requires(source != null);
            Contract.Requires(srcOffset >= 0);
            Contract.Requires(destination != null);
            Contract.Requires(destOffset >= 0);
            Contract.Requires(pixelCount >= 0);
            Contract.Requires(srcOffset + 4 * pixelCount <= source.Length);
            Contract.Requires(destOffset + 3 * pixelCount <= destination.Length);

            fixed (byte* sourcePtr = source, destinationPtr = destination)
            {
                var sourceStart = sourcePtr + srcOffset;
                var destinationStart = destinationPtr + destOffset;
                var sourceEnd = sourceStart + 4 * pixelCount;
                var src = sourceStart;
                var dest = destinationStart;
                while (src < sourceEnd)
                {
                    *(dest++) = *(src++);
                    *(dest++) = *(src++);
                    *(dest++) = *(src++);
                    src++;
                }
            }
        }

        public static void FlipVertical(byte[] source, int srcOffset, byte[] destination, int destOffset, int height, int stride)
        {
            Contract.Requires(source != null);
            Contract.Requires(destination != null);
            Contract.Requires(height >= 0);
            Contract.Requires(stride > 0);
            Contract.Requires(srcOffset + stride * height <= source.Length);
            Contract.Requires(destOffset + stride * height <= destination.Length);

            var src = srcOffset;
            var dest = destOffset + (height - 1) * stride;
            for (var y = 0; y < height; y++)
            {
                Buffer.BlockCopy(source, src, destination, dest, stride);
                src += stride;
                dest -= stride;
            }
        }
    }
}
