using System;

namespace SharpAvi
{
    /// <summary>
    /// Auxiliary methods helping to deal with AVI files.
    /// </summary>
    public static class AviUtils
    {
        /// <summary>
        /// Splits frame rate value to integer <c>rate</c> and <c>scale</c> values used in some AVI headers
        /// and VfW APIs.
        /// </summary>
        /// <param name="frameRate">
        /// Frame rate. Rounded to 3 fractional digits.
        /// </param>
        /// <param name="rate">
        /// When the method returns, contains rate value.
        /// </param>
        /// <param name="scale">
        /// When the method returns, contains scale value.
        /// </param>
        public static void SplitFrameRate(decimal frameRate, out uint rate, out uint scale)
        {
            if (Decimal.Round(frameRate) == frameRate)
            {
                rate = (uint)Decimal.Truncate(frameRate);
                scale = 1;
            }
            else if (Decimal.Round(frameRate, 1) == frameRate)
            {
                rate = (uint)Decimal.Truncate(frameRate * 10m);
                scale = 10;
            }
            else if (Decimal.Round(frameRate, 2) == frameRate)
            {
                rate = (uint)Decimal.Truncate(frameRate * 100m);
                scale = 100;
            }
            else
            {
                rate = (uint)Decimal.Truncate(frameRate * 1000m);
                scale = 1000;
            }

            // Make mutually prime (needed for some hardware players)
            while (rate % 2 == 0 && scale % 2 == 0)
            {
                rate /= 2;
                scale /= 2;
            }
            while (rate % 5 == 0 && scale % 5 == 0)
            {
                rate /= 5;
                scale /= 5;
            }
        }
    }
}
