using System;

namespace SharpAvi.Codecs
{
    internal class BitmapUtils
    {
        public static unsafe void Bgr32ToBgr24(byte[] source, int srcOffset, byte[] destination, int destOffset, int pixelCount)
        {
            Argument.IsNotNull(source, nameof(source));
            Argument.IsNotNegative(srcOffset, nameof(srcOffset));
            Argument.IsNotNull(destination, nameof(destination));
            Argument.IsNotNegative(destOffset, nameof(destOffset));
            Argument.IsPositive(pixelCount, nameof(pixelCount));
            Argument.ConditionIsMet(srcOffset + 4 * pixelCount <= source.Length,
                "Source end offset exceeds the source length.");
            Argument.ConditionIsMet(destOffset + 3 * pixelCount <= destination.Length,
                "Destination end offset exceeds the destination length.");

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
            Argument.IsNotNull(source, nameof(source));
            Argument.IsNotNull(destination, nameof(destination));
            Argument.IsPositive(height, nameof(height));
            Argument.IsPositive(stride, nameof(stride));
            Argument.ConditionIsMet(srcOffset + stride * height <= source.Length,
                "Source end offset exceeds the source length.");
            Argument.ConditionIsMet(destOffset + stride * height <= destination.Length,
                "Destination end offset exceeds the destination length.");

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
