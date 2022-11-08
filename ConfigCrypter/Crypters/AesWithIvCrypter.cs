using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using DevAttic.ConfigCrypter.Extensions;

namespace DevAttic.ConfigCrypter.Crypters
{
    public class AesWithIvCrypter : ICrypter
    {
        protected const int KeySize256Bit = 32;
        protected readonly Aes Aes;
        protected readonly Encoding Encoding;

        protected AesWithIvCrypter(string secretKey, Encoding encoding = null) : this(secretKey, encoding, null)
        {
        }

        public AesWithIvCrypter(string secretKey, string iv, Encoding encoding = null) : this(secretKey, encoding, iv)
        {
            if (string.IsNullOrWhiteSpace(iv))
            {
                throw new ArgumentException("iv can't not be null or empty or whitespace either", nameof(iv));
            }
        }

        private AesWithIvCrypter(string secretKey, Encoding encoding = null, string iv = null)
        {
            Encoding = encoding ?? Encoding.UTF8;
            Aes = Aes.Create();
            Aes.Mode = CipherMode.CFB;
            Aes.Padding = PaddingMode.PKCS7;
            Aes.Key = FitKeyLength(Encoding.GetBytes(secretKey), KeySize256Bit);
            if (string.IsNullOrWhiteSpace(iv))
            {
                Aes.GenerateIV();
            }
            else
            {
                Aes.IV = FitIvLength(Encoding.GetBytes(iv));
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Aes?.Dispose();
            }
        }

        public string DecryptString(string value)
        {
            return Encoding.GetString(Decrypt(Convert.FromBase64String(value)));
        }

        public string EncryptString(string value)
        {
            return Convert.ToBase64String(Encrypt(Encoding.GetBytes(value)));
        }

        protected virtual byte[] Encrypt(byte[] bytes)
        {
            byte[] r;
            using (var mem = new MemoryStream())
            {
                using (var crypto = new CryptoStream(mem, Aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    crypto.Write(bytes, 0, bytes.Length);
                }

                r = mem.ToArray();
            }

            return r;
        }

        protected virtual byte[] Decrypt(byte[] bytes)
        {
            byte[] r;
            using (var mem = new MemoryStream(bytes))
            {
                using (var crypto = new CryptoStream(mem, Aes.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    r = crypto.ReadToEnd();
                }
            }

            return r;
        }

        protected static byte[] FitKeyLength(byte[] key, int keySize)
        {
            var keyBytes = new byte[key.Length];
            Array.Copy(key, keyBytes, key.Length);
            keyBytes = FitBytesInLength(keyBytes, keySize);
            return keyBytes;
        }

        protected static byte[] FitBytesInLength(byte[] bytes, int length)
        {
            while (bytes.Length < length)
            {
                bytes = bytes.Concat(bytes).ToArray();
            }

            if (bytes.Length > length)
            {
                bytes = bytes.AsSpan().Slice(0, length).ToArray();
            }

            return bytes;
        }

        protected static byte[] FitIvLength(byte[] iv)
        {
            var ivBytes = new byte[16];
            iv = FitBytesInLength(iv, 16);
            Array.Copy(iv, ivBytes, ivBytes.Length);
            return ivBytes;
        }
    }
}