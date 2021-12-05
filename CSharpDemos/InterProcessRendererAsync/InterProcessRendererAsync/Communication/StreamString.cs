using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace InterProcessRendererAsync.Communication
{
    class StreamString
    {
        private Stream ioStream;
        private UnicodeEncoding streamEncoding;

        public StreamString(Stream ioStream)
        {
            this.ioStream = ioStream;
            streamEncoding = new UnicodeEncoding();

        }

        public string ReadString()
        {
            int len = 0;

            len = ioStream.ReadByte() << 24;

            len += ioStream.ReadByte() << 16;

            len += ioStream.ReadByte() << 8;

            len += ioStream.ReadByte();

            byte[] inBuffer = new byte[len];

            ioStream.Read(inBuffer, 0, len);

            return streamEncoding.GetString(inBuffer);
        }

        public int WriteString(string outString)
        {
            byte[] outBuffer = streamEncoding.GetBytes(outString);
            int len = outBuffer.Length;
            ioStream.WriteByte((byte)(len >> 24));
            ioStream.WriteByte((byte)(len >> 16));
            ioStream.WriteByte((byte)(len >> 8));
            ioStream.WriteByte((byte)len);
            ioStream.Write(outBuffer, 0, len);
            ioStream.Flush();
            return outBuffer.Length + 4;
        }
    }
}
