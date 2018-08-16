using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Words.Util
{
    public static class StreamExtensions
    {
        public static byte[] ToByteArray(this Stream stream)
        {
            var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
