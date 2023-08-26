using System.IO.Compression;
using System.Text;

namespace BlazorAssistant.Storage.Utils
{
    internal static class CompressionUtilss
    {
        /// <summary>
        /// Compresses a string using Gzip.
        /// </summary>
        /// <param name="value">The value to be compressed</param>
        /// <returns>The compressed version of the string.</returns>
        /// <exception cref="ArgumentException">If the value is null or empty.</exception>
        internal static async Task<string> Gzip(this string? value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException($"Provided value can't be null or empty.");

            try
            {
                byte[] compressedBytes;

                using (var ms = new MemoryStream())
                {
                    using (GZipStream gzip = new(ms, CompressionMode.Compress))
                    {
                        byte[] data = Encoding.UTF8.GetBytes(value);
                        await gzip.WriteAsync(data);
                    }

                    compressedBytes = ms.ToArray();
                }

                return Convert.ToBase64String(compressedBytes);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Decompresses a Gzipped string.
        /// </summary>
        /// <param name="value">The string compressed string to be decompressed.</param>
        /// <returns>The decompressed version of the string.</returns>
        /// <exception cref="ArgumentException">If the value is null or empty.</exception>
        internal static async Task<string> UnGzip(this string? value)
        {
            try
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException($"Provided value can't be null or empty.");

                byte[] compressedBytes = Convert.FromBase64String(value);
                byte[] decompressedBytes;

                using (MemoryStream ms = new(compressedBytes))
                {
                    await using GZipStream gzip = new(ms, CompressionMode.Decompress);
                    await using MemoryStream outputMemoryStream = new();
                    await gzip.CopyToAsync(outputMemoryStream);
                    decompressedBytes = outputMemoryStream.ToArray();
                }

                string storedValue;
                using (MemoryStream decompressStream = new(decompressedBytes))
                {
                    using StreamReader reader = new(decompressStream);
                    storedValue = reader.ReadToEnd();
                }

                return storedValue;
            }
            catch
            {
                throw;
            }
        }
    }
}
