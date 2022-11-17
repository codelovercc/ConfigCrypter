using System.Linq;
using CommandLine;
using ConfigCrypter.Console.Options;
using DevAttic.ConfigCrypter;
using DevAttic.ConfigCrypter.CertificateLoaders;
using DevAttic.ConfigCrypter.ConfigCrypters.Json;
using DevAttic.ConfigCrypter.Crypters;

namespace ConfigCrypter.Console
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<EncryptOptions, DecryptOptions, ChangeEncryptionOptions>(args)
                .WithParsed<EncryptOptions>(opts =>
                {
                    var crypter = CreateCrypter(opts);
                    if (!string.IsNullOrWhiteSpace(opts.Key))
                    {
                        crypter.EncryptKeyInFile(opts.ConfigFile, opts.Key);
                    }
                    else
                    {
                        crypter.EncryptFile(opts.ConfigFile, opts.Keys.ToList(), opts.KeyPrefix);
                    }
                })
                .WithParsed<DecryptOptions>(opts =>
                {
                    var crypter = CreateCrypter(opts);
                    if (!string.IsNullOrWhiteSpace(opts.Key))
                    {
                        crypter.DecryptKeyInFile(opts.ConfigFile, opts.Key);
                    }
                    else
                    {
                        crypter.DecryptFile(opts.ConfigFile, opts.Keys.ToList(), opts.KeyPrefix);
                    }
                })
                .WithParsed<ChangeEncryptionOptions>(options =>
                {
                    var crypter = CreateCrypter(options);
                    if (!string.IsNullOrWhiteSpace(options.Key))
                    {
                        crypter.ReEncryptKeyInFile(options.ConfigFile,
                            CreateConfigCrypter(options.SecretKeyNew, options.SecretIvNew, options.CertificatePathNew,
                                options.CertificatePasswordNew, options.CertSubjectNameNew), options.Key);
                    }
                    else
                    {
                        crypter.ReEncryptFile(options.ConfigFile,
                            CreateConfigCrypter(options.SecretKeyNew, options.SecretIvNew, options.CertificatePathNew,
                                options.CertificatePasswordNew, options.CertSubjectNameNew), options.Keys.ToList(),
                            options.KeyPrefix);
                    }
                });
        }

        private static JsonConfigCrypter CreateConfigCrypter(string secretKey, string secretIv, string certificatePath,
            string certificatePassword, string certSubjectName)
        {
            if (!string.IsNullOrEmpty(secretKey))
            {
                var aesCrypter = string.IsNullOrEmpty(secretIv)
                    ? new AesCrypter(secretKey)
                    : new AesWithIvCrypter(secretKey, secretIv);
                return new JsonConfigCrypter(aesCrypter);
            }

            ICertificateLoader certLoader = null;
            if (!string.IsNullOrEmpty(certificatePath))
            {
                certLoader =
                    new FilesystemCertificateLoader(certificatePath, certificatePassword);
            }
            else if (!string.IsNullOrEmpty(certSubjectName))
            {
                certLoader = new StoreCertificateLoader(certSubjectName);
            }

            var configCrypter = new JsonConfigCrypter(new RSACrypter(certLoader));
            return configCrypter;
        }

        private static ConfigFileCrypter CreateCrypter(CommandlineOptions options)
        {
            var configCrypter = CreateConfigCrypter(options.SecretKey, options.SecretIv, options.CertificatePath,
                options.CertificatePassword, options.CertSubjectName);

            var fileCrypter = new ConfigFileCrypter(configCrypter, new ConfigFileCrypterOptions()
            {
                ReplaceCurrentConfig = options.Replace
            });

            return fileCrypter;
        }
    }
}