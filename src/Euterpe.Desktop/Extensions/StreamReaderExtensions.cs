namespace Euterpe.Extensions;

public static class StreamReaderExtensions
{
    extension(StreamReader streamReader)
    {
        /// <summary>
        ///     Read lines using <see cref="StreamReader" /> asynchronously, with start line and end line.
        ///     Line numbers start at 1
        /// </summary>
        /// <param name="startLine"></param>
        /// <param name="endLine"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<string?[]> ReadLineAsync(int startLine, int endLine)
        {
            if (startLine > endLine || startLine < 1)
            {
                throw new ArgumentException("startLine must be at least 1 and not greater than endLine");
            }

            var lines = new List<string?>();

            for (var i = 0; i < startLine; i++)
            {
                await streamReader.ReadLineAsync().ConfigureAwait(false);
            }

            for (var i = startLine; i <= endLine; i++)
            {
                var line = await streamReader.ReadLineAsync().ConfigureAwait(false);
                if (line is null)
                {
                    break;
                }

                lines.Add(line);
            }

            return lines.ToArray();
        }

        /// <summary>
        ///     Read lines using <see cref="StreamReader" /> asynchronously, with range
        ///     Line numbers start at 1
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<string?[]> ReadLineAsync(Range range)
        {
            var startLine = range.Start.Value;
            var endLine = range.End.Value;
            if (startLine > endLine || startLine < 1)
            {
                throw new ArgumentException("startLine must be at least 1 and not greater than endLine");
            }

            var lines = new List<string?>();

            for (var i = 0; i < startLine; i++)
            {
                await streamReader.ReadLineAsync().ConfigureAwait(false);
            }

            for (var i = startLine; i <= endLine; i++)
            {
                var line = await streamReader.ReadLineAsync().ConfigureAwait(false);
                if (line is null)
                {
                    break;
                }

                lines.Add(line);
            }

            return lines.ToArray();
        }
    }
}
