using System.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace Euterpe.Shared.Extensions;

public static class StringExtensions
{
    private static readonly SearchValues<char> InvalidChars = SearchValues.Create(Path.GetInvalidFileNameChars());
    private static readonly SearchValues<char> ShellSpecialChars = SearchValues.Create('\\', '"', '$', '`');

    /// <summary>
    ///     Check whether the string is null or empty
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? str) => string.IsNullOrEmpty(str);

    /// <summary>
    ///     Check whether the string is null, empty or consists of only white-space characters
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? str) => string.IsNullOrWhiteSpace(str);

    extension(string str)
    {
        /// <summary>
        ///     Replace "\\" with "\" to normalize slashes in path
        /// </summary>
        /// <returns></returns>
        public string NormalizeSlashes() => str.Replace(@"\\", @"\");

        /// <summary>
        ///     Parse level from string
        /// </summary>
        /// <returns></returns>
        public int ParseLevel() => !int.TryParse(str, out var level) ? 0 : level;

        /// <summary>
        ///     Remove invalid chars for file names from string
        /// </summary>
        /// <returns></returns>
        public string RemoveInvalidFileNameChars()
        {
            var span = str.AsSpan();
            if (!span.ContainsAny(InvalidChars))
            {
                return str;
            }

            return string.Create(str.Length, str, static (dest, src) =>
            {
                for (var i = 0; i < src.Length; i++)
                {
                    var c = src[i];
                    dest[i] = InvalidChars.Contains(c) ? '_' : c;
                }
            });
        }

        public string EscapeDesktopExecArgument()
        {
            var span = str.AsSpan();
            if (!span.ContainsAny(ShellSpecialChars))
            {
                return str;
            }

            var specialCount = span.CountAny(ShellSpecialChars);

            return string.Create(str.Length + specialCount, str, static (dest, src) =>
            {
                var i = 0;
                foreach (var c in src)
                {
                    if (c is '\\' or '"' or '$' or '`')
                    {
                        dest[i++] = '\\';
                    }

                    dest[i++] = c;
                }
            });
        }
    }
}
