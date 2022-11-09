using System;
using System.Collections.Generic;
using DevAttic.ConfigCrypter.CertificateLoaders;
using DevAttic.ConfigCrypter.Crypters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace DevAttic.ConfigCrypter.ConfigProviders.Json
{
    /// <summary>
    /// ConfigurationSource for encrypted JSON config files. ReloadOnChange default is true
    /// </summary>
    public class EncryptedJsonConfigSource : JsonConfigurationSource
    {
        /// <summary>
        /// 加解密类型，默认为非对称类型，如果使用<see cref="CryptType.Asymmetric"/>则需要设置<see cref="CertificatePath"/>（将从文件加载证书）,<see cref="CertificatePassword"/>（若证书有密码保护则需要）
        /// 或者<see cref="CertificateSubjectName"/>（将从证书存储中读取证书内容）
        /// 或者<see cref="CertificateRawData"/>（使用原始证书内容),<see cref="CertificatePassword"/>（若证书有密码保护则需要）
        /// <br/>
        /// 如果使用<see cref="CryptType.Symmetric"/>则需要设置<see cref="SecretKey"/>，<see cref="SecretIv"/>是可选设置
        /// </summary>
        public CryptType Type { get; set; } = CryptType.Asymmetric;

        /// <summary>
        /// AES加解密使用的密钥
        /// </summary>
        public string SecretKey { get; set; }

        /// <summary>
        /// AES加解密使用的向量
        /// </summary>
        public string SecretIv { get; set; }

        /// <summary>
        /// A certificate loader instance. Custom loaders can be used. the loader will not be used when <see cref="CryptType"/> is <see cref="CryptType.Symmetric"/> 
        /// </summary>
        public ICertificateLoader CertificateLoader { get; set; }

        /// <summary>
        /// 证书原始内容
        /// </summary>
        public byte[] CertificateRawData { get; set; }

        /// <summary>
        /// 在配置值中表示已经加密的前缀，带有该前缀的值将会被解密，该属性指示的要解密的值与<see cref="KeysToDecrypt"/>中设置的要解密的键的值都将会被解密
        /// </summary>
        public string KeyValueToDecryptPrefix { get; set; }

        /// <summary>
        /// The fully qualified path of the certificate.
        /// </summary>
        public string CertificatePath { get; set; }

        /// <summary>
        /// The subject name of the certificate (Issued for).
        /// </summary>
        public string CertificateSubjectName { get; set; }

        /// <summary>
        /// The password of the certificate or null, if the certificate has no password.
        /// </summary>
        public string CertificatePassword { get; set; } = null;

        /// <summary>
        /// Factory function that is used to create an instance of the crypter.
        /// The default factory uses the RSACrypter and passes it the given certificate loader or AesCrypter according CryptType is Symmetric.
        /// </summary>
        public Func<EncryptedJsonConfigSource, ICrypter> CrypterFactory { get; set; } =
            cfg =>
            {
                switch (cfg.Type)
                {
                    case CryptType.Asymmetric:
                        return new RSACrypter(cfg.CertificateLoader);
                    case CryptType.Symmetric:
                        if (string.IsNullOrWhiteSpace(cfg.SecretKey))
                        {
                            throw new InvalidOperationException(
                                "SecretKey has to be provided when using symmetric encrypt");
                        }

                        return string.IsNullOrWhiteSpace(cfg.SecretIv)
                            ? new AesCrypter(cfg.SecretKey)
                            : new AesWithIvCrypter(cfg.SecretKey, cfg.SecretIv);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(cfg.Type));
                }
            };

        /// <summary>
        /// List of keys that should be decrypted. Hierarchical keys need to be separated by colon.
        /// <code>Example: "Nested:Key"</code>
        /// </summary>
        public List<string> KeysToDecrypt { get; set; } = new List<string>();

        public EncryptedJsonConfigSource()
        {
            ReloadOnChange = true;
        }

        /// <summary>
        /// Creates an instance of the EncryptedJsonConfigProvider.
        /// </summary>
        /// <param name="builder">IConfigurationBuilder instance.</param>
        /// <returns>An EncryptedJsonConfigProvider instance.</returns>
        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            base.Build(builder);
            return new EncryptedJsonConfigProvider(this);
        }
    }
}