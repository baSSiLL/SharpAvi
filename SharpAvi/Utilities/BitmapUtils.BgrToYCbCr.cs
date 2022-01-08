#if NET5_0_OR_GREATER
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpAvi.Utilities
{
    partial class BitmapUtils
    {
        private static readonly BgrToYCbCrConverter bgrToYCbCrConverter = new();

        public static void Bgr32ToYCbCr8(ReadOnlySpan<byte> source, Span<byte> destination, int pixelCount)
        {
            bgrToYCbCrConverter.ConvertBgr32ToYCbCr8(source, destination, pixelCount);
        }

        /// <summary>
        /// Converter from BGR format to YCbCr.
        /// </summary>
        /// <remarks>
        /// Based on https://github.com/yigolden/JpegLibrary/blob/main/apps/JpegEncode/JpegRgbToYCbCrConverter.cs
        /// </remarks>
        private sealed class BgrToYCbCrConverter
        {
            private readonly int[] yRTable = new int[256];
            private readonly int[] yGTable = new int[256];
            private readonly int[] yBTable = new int[256];
            private readonly int[] cbRTable = new int[256];
            private readonly int[] cbGTable = new int[256];
            private readonly int[] cbBTable = new int[256];
            private readonly int[] crGTable = new int[256];
            private readonly int[] crBTable = new int[256];

            private const int SCALE_BITS = 16;
            private const int CB_CR_OFFSET = 128 << SCALE_BITS;
            private const int HALF = 1 << (SCALE_BITS - 1);

            public BgrToYCbCrConverter()
            {
                for (int i = 0; i < 256; i++)
                {
                    // The values for the calculations are left scaled up since we must add them together before rounding.
                    yRTable[i] = Fix(0.299F) * i;
                    yGTable[i] = Fix(0.587F) * i;
                    yBTable[i] = (Fix(0.114F) * i) + HALF;
                    cbRTable[i] = (-Fix(0.168735892F)) * i;
                    cbGTable[i] = (-Fix(0.331264108F)) * i;

                    // We use a rounding fudge - factor of 0.5 - epsilon for Cb and Cr.
                    // This ensures that the maximum output will round to 255
                    // not 256, and thus that we don't have to range-limit.
                    //
                    // B=>Cb and R=>Cr tables are the same
                    cbBTable[i] = (Fix(0.5F) * i) + CB_CR_OFFSET + HALF - 1;

                    crGTable[i] = (-Fix(0.418687589F)) * i;
                    crBTable[i] = (-Fix(0.081312411F)) * i;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static int Fix(float x)
            {
                return (int)((x * (1L << SCALE_BITS)) + 0.5F);
            }

            public void ConvertBgr32ToYCbCr8(ReadOnlySpan<byte> source, Span<byte> destination, int count)

            {
                Argument.ConditionIsMet(source.Length >= 4 * count, "Source end offset exceeds the source length.");
                Argument.ConditionIsMet(destination.Length >= 3 * count, "Destination end offset exceeds the destination length.");

                ref byte sourceRef = ref MemoryMarshal.GetReference(source);
                ref byte destinationRef = ref MemoryMarshal.GetReference(destination);

                byte r, g, b;

                for (int i = 0; i < count; i++)
                {
                    b = sourceRef;
                    g = Unsafe.Add(ref sourceRef, 1);
                    r = Unsafe.Add(ref sourceRef, 2);

                    destinationRef = (byte)((yRTable[r] + yGTable[g] + yBTable[b]) >> SCALE_BITS);
                    Unsafe.Add(ref destinationRef, 1) = (byte)((cbRTable[r] + cbGTable[g] + cbBTable[b]) >> SCALE_BITS);
                    Unsafe.Add(ref destinationRef, 2) = (byte)((cbBTable[r] + crGTable[g] + crBTable[b]) >> SCALE_BITS);

                    sourceRef = ref Unsafe.Add(ref sourceRef, 4);
                    destinationRef = ref Unsafe.Add(ref destinationRef, 3);
                }
            }
        }
    }
}
#endif