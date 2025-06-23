
using System.IO.Compression;
using System.Text;

namespace GCloudShared.Shared
{
    public static class CompressString
    {
      
        public static string Compress(string input)
        {
            byte[] inputBytes = Encoding.UTF32.GetBytes(input);

            using (MemoryStream outputStream = new MemoryStream())
            {
                using (GZipStream gzipStream = new GZipStream(outputStream, CompressionMode.Compress))
                {
                    gzipStream.Write(inputBytes, 0, inputBytes.Length);
                }

                byte[] compressedBytes = outputStream.ToArray();
                return Convert.ToBase64String(compressedBytes);
            }
        }

        public static string DecompressString(string compressedInput)
        {
            byte[] compressedBytes = Convert.FromBase64String(compressedInput);

            using (MemoryStream inputStream = new MemoryStream(compressedBytes))
            {
                using (GZipStream gzipStream = new GZipStream(inputStream, CompressionMode.Decompress))
                {
                    using (MemoryStream outputStream = new MemoryStream())
                    {
                        byte[] buffer = new byte[4096];
                        int bytesRead;

                        while ((bytesRead = gzipStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            outputStream.Write(buffer, 0, bytesRead);
                        }

                        byte[] decompressedBytes = outputStream.ToArray();
                        return Encoding.UTF32.GetString(decompressedBytes);
                    }
                }
            }
        }
    }
}
