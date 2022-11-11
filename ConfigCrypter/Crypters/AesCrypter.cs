using System;
using System.Text;

namespace DevAttic.ConfigCrypter.Crypters
{
    public class AesCrypter : AesWithIvCrypter
    {
        public AesCrypter(string secretKey, Encoding encoding = null) : base(secretKey, encoding)
        {
        }

        protected override byte[] Encrypt(byte[] bytes)
        {
            var r = base.Encrypt(bytes);
            var riv = new byte[Aes.IV.Length + r.Length];
            var index = 0;
            riv[index] = r[index];
            const int copiedLength = 1;
            index += copiedLength;
            Array.Copy(Aes.IV, 0, riv, index, Aes.IV.Length);
            index += Aes.IV.Length;
            Array.Copy(r, copiedLength, riv, index, r.Length - copiedLength);
            return riv;
        }

        protected override byte[] Decrypt(byte[] bytes)
        {
            var iv = bytes.AsSpan().Slice(1, Aes.IV.Length).ToArray();
            Aes.IV = iv;
            var encryptedBytes = new byte[bytes.Length - Aes.IV.Length];
            var index = 0;
            encryptedBytes[index] = bytes[index];
            index += Aes.IV.Length + 1;
            Array.Copy(bytes, index, encryptedBytes, 1, bytes.Length - index);
            return base.Decrypt(encryptedBytes);
        }
    }
}