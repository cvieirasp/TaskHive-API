namespace TaskHive.Domain.Extensions
{
    public static class EnumExtensions
    {
        public static T ToEnum<T>(this string value, bool ignoreCase = true) where T : struct, Enum
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(nameof(value));

            if (Enum.TryParse<T>(value, ignoreCase, out var result))
                return result;

            throw new ArgumentException($"'{value}' is not a valid value for enum {typeof(T).Name}");
        }
    }
}
