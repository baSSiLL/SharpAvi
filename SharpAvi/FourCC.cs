using System.Diagnostics.Contracts;
using System.Linq;

namespace SharpAvi
{
    /// <summary>
    /// Represents four character code (FOURCC).
    /// </summary>
    /// <remarks>
    /// FOURCCs are used widely across AVI format.
    /// </remarks>
    public struct FourCC
    {
        private readonly uint valueDWord;
        private readonly string valueString;

        /// <summary>
        /// Creates a new instance of <see cref="FourCC"/> with an integer value.
        /// </summary>
        /// <param name="value">Integer value of FOURCC.</param>
        public FourCC(uint value)
        {
            valueDWord = value;
            valueString = new string
                              (
                                  new[]
                                  {
                                      (char)(value & 0xFF),
                                      (char)((value & 0xFF00) >> 8),
                                      (char)((value & 0xFF0000) >> 16),
                                      (char)((value & 0xFF000000U) >> 24)
                                  }
                              );
        }

        /// <summary>
        /// Creates a new instance of <see cref="FourCC"/> with a string value.
        /// </summary>
        /// <param name="value">
        /// String value of FOURCC.
        /// Should be not longer than 4 characters, all of them are printable ASCII characters.
        /// </param>
        /// <remarks>
        /// If the value of <paramref name="value"/> is shorter than 4 characters, it is right-padded with spaces.
        /// </remarks>
        public FourCC(string value)
        {
            Contract.Requires(value != null);
            Contract.Requires(value.Length <= 4);
            // Allow only printable ASCII characters
            Contract.Requires(Contract.ForAll(value, c => ' ' <= c && c <= '~'));

            valueString = value.PadRight(4);
            valueDWord = (uint)valueString[0] + ((uint)valueString[1] << 8) + ((uint)valueString[2] << 16) + ((uint)valueString[3] << 24);
        }

        /// <summary>
        /// Returns string representation of this instance.
        /// </summary>
        /// <returns>
        /// String value if all bytes are printable ASCII characters. Otherwise, the hexadecimal representation of integer value.
        /// </returns>
        public override string ToString()
        {
            var isPrintable = valueString.All(c => ' ' <= c && c <= '~');
            return isPrintable ? valueString : valueDWord.ToString("X8");
        }

        /// <summary>
        /// Gets hash code of this instance.
        /// </summary>
        public override int GetHashCode()
        {
            return valueDWord.GetHashCode();
        }

        /// <summary>
        /// Determines whether this instance is equal to other object.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is FourCC)
            {
                return (FourCC)obj == this;
            }
            else
            {
                return base.Equals(obj);
            }
        }


        /// <summary>
        /// Converts an integer value to <see cref="FourCC"/>.
        /// </summary>
        public static implicit operator FourCC(uint value)
        {
            return new FourCC(value);
        }

        /// <summary>
        /// Converts a string value to <see cref="FourCC"/>.
        /// </summary>
        public static implicit operator FourCC(string value)
        {
            return new FourCC(value);
        }

        /// <summary>
        /// Gets the integer value of <see cref="FourCC"/> instance.
        /// </summary>
        public static explicit operator uint(FourCC value)
        {
            return value.valueDWord;
        }

        /// <summary>
        /// Gets the string value of <see cref="FourCC"/> instance.
        /// </summary>
        public static explicit operator string(FourCC value)
        {
            return value.valueString;
        }

        /// <summary>
        /// Determines whether two instances of <see cref="FourCC"/> are equal.
        /// </summary>
        public static bool operator ==(FourCC value1, FourCC value2)
        {
            return value1.valueDWord == value2.valueDWord;
        }

        /// <summary>
        /// Determines whether two instances of <see cref="FourCC"/> are not equal.
        /// </summary>
        public static bool operator !=(FourCC value1, FourCC value2)
        {
            return !(value1 == value2);
        }
    }
}
