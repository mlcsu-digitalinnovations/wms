using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WmsHub.Business.Helpers
{
  public static class CompressionHelper
  {
    private static int BUFFER_SIZE = 64 * 1024; //64kB

    public static byte[] CompressFromBase64(this string base64)
    {
      var plainTextBytes = Encoding.UTF8.GetBytes(base64);
      return plainTextBytes.Compress();
    }
    public static byte[] Compress(this byte[] input)
    {
      if (input == null)
        throw new ArgumentNullException("Byte Array was null");

      if (input.Length < 1)
        throw new ArgumentException("Byte Array was empty");

      using (MemoryStream ms = new MemoryStream())
      {
        using (BufferedStream bs =
          new BufferedStream(new GZipStream(ms, CompressionMode.Compress),
            BUFFER_SIZE))
        {
          bs.Write(input, 0, input.Length);
        }

        return ms.ToArray();
      }
    }

    public static byte[] Decompress(this byte[] input)
    {
      if (input == null)
        throw new ArgumentNullException("Byte Array was empty");

      if (input.Length < 1)
        throw new ArgumentException("Byte Array was empty");

      using (MemoryStream cMs = new MemoryStream(input))
      {
        using (var dMs = new MemoryStream())
        {
          using (BufferedStream bs = new BufferedStream(new GZipStream(cMs,
            CompressionMode.Decompress), BUFFER_SIZE))
          {
            bs.CopyTo(dMs);
          }
          return dMs.ToArray();
        }
      }

    }
  }
}
