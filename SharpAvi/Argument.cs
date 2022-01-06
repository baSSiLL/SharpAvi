using System;

namespace SharpAvi
{
    /// <summary>
    /// An utility class for argument checks.
    /// </summary>
    /// <remarks>
    /// The methods are not extensions to make argument checks look more explicit
    /// (at the expense of a bit more verbosity).
    /// </remarks>
    internal static class Argument
    {
        public static void IsNotNull(object value, string name)
        {
            if (value is null)
                throw new ArgumentNullException(name);
        }

        public static void IsNotNull<T>(T? value, string name) where T : struct
        {
            if (value is null)
                throw new ArgumentNullException(name);
        }

        public static void IsNotNullOrEmpty(string value, string name)
        {
            IsNotNull(value, name);

            if (value.Length == 0)
                throw new ArgumentOutOfRangeException(name, "A non-empty string is expected.");
        }

        public static void IsNotNegative(int value, string name)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(name, message: "A non-negative number is expected.");
        }

        public static void IsPositive(int value, string name)
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(name, message: "A positive number is expected.");
        }

        public static void IsInRange(int value, int min, int max, string name)
        {
            if (value < min || max < value)
                throw new ArgumentOutOfRangeException(name, message: $"A value in the range [{min}..{max}] is expected.");
        }

        public static void IsNotNegative(double value, string name)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(name, message: "A non-negative number is expected.");
        }

        public static void IsPositive(double value, string name)
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(name, message: "A positive number is expected.");
        }

        public static void IsNotNegative(decimal value, string name)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(name, message: "A non-negative number is expected.");
        }

        public static void IsPositive(decimal value, string name)
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(name, message: "A positive number is expected.");
        }

        public static void IsEnumMember<TEnum>(TEnum value, string name) where TEnum : struct
        {
            if (!Enum.IsDefined(typeof(TEnum), value))
                throw new ArgumentOutOfRangeException(name, message: $"A member of {typeof(TEnum).Name} is expected.");
        }

        public static void Meets(bool condition, string name, string failureDescription = null)
        {
            if (!condition)
                throw new ArgumentOutOfRangeException(name, message: failureDescription);
        }

        public static void ConditionIsMet(bool condition, string failureDescription)
        {
            if (!condition)
                throw new ArgumentException(failureDescription);
        }
    }
}
