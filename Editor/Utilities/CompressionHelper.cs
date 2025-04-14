using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace Editor.Utilities
{
    static class CompressionHelper
    {
        public static byte[] Compress(byte[] data)
        {
            Debug.Assert(data?.Length > 0);

            byte[] compressedData = null;

            using (var output = new MemoryStream())
            {
                using (var compressor = new DeflateStream(output, CompressionLevel.Optimal, true))
                {
                    compressor.Write(data, 0, data.Length);
                }

                compressedData = output.ToArray();
            }

            return compressedData;
        }

        public static byte[] Decompress(byte[] data)
        {
            Debug.Assert(data?.Length > 0);

            byte[] decompressedData = null;

            using (var output = new MemoryStream())
            {
                using (var compressonr = new DeflateStream(new MemoryStream(data), CompressionMode.Decompress))
                {
                    compressonr.CopyTo(output);
                }

                decompressedData = output.ToArray();
            }

            return decompressedData;
        }
    }
}
