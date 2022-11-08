using System.IO;

namespace DevAttic.ConfigCrypter.Extensions
{
    public static class StreamExtensions
    {
        private const int BlockSize = 1024;

        /// <summary>
        /// 读取所有数据
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static byte[] ReadToEnd(this Stream stream)
        {
            var buf = new byte[BlockSize];
            using (var mem = new MemoryStream())
            {
                int c;
                while ((c = stream.Read(buf, 0, buf.Length)) != 0)
                {
                    mem.Write(buf, 0, c);
                }

                return mem.ToArray();
            }
        }
    }
}