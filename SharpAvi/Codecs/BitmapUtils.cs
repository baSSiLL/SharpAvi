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
                Bgr32ToBgr24(sourcePtr, srcOffset, destinationPtr, destOffset, pixelCount);
            }
        }

#if NET5_0_OR_GREATER
        public static unsafe void Bgr32ToBgr24(ReadOnlySpan<byte> source, Span<byte> destination, int pixelCount)
        {
            Argument.IsPositive(pixelCount, nameof(pixelCount));
            Argument.ConditionIsMet(4 * pixelCount <= source.Length,
                "Source end offset exceeds the source length.");
            Argument.ConditionIsMet(3 * pixelCount <= destination.Length,
                "Destination end offset exceeds the destination length.");

            fixed (byte* sourcePtr = source, destinationPtr = destination)
            {
                Bgr32ToBgr24(sourcePtr, 0, destinationPtr, 0, pixelCount);
            }
        }
#endif

        private static unsafe void Bgr32ToBgr24(byte* sourcePtr, int srcOffset, byte* destinationPtr, int destOffset, int pixelCount)
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

#if NET5_0_OR_GREATER
        public static void FlipVertical(ReadOnlySpan<byte> source, Span<byte> destination, int height, int stride)
        {
            Argument.IsPositive(height, nameof(height));
            Argument.IsPositive(stride, nameof(stride));
            Argument.ConditionIsMet(stride * height <= source.Length,
                "Source end offset exceeds the source length.");
            Argument.ConditionIsMet(stride * height <= destination.Length,
                "Destination end offset exceeds the destination length.");

            for (var y = 0; y < height; y++)
            {
                var srcOffset = y * stride;
                var destOffset = (height - 1 - y) * stride;
                source.Slice(srcOffset, stride).CopyTo(destination.Slice(destOffset));
            }
        }
#endif

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
