﻿using System.Linq;
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
            Parser.Default.ParseArguments<EncryptOptions, DecryptOptions>(args)
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
                });
        }

        private static ConfigFileCrypter CreateCrypter(CommandlineOptions options)
        {
            ICertificateLoader certLoader = null;

            if (!string.IsNullOrEmpty(options.SecretKey))
            {
                var aesCrypter = string.IsNullOrEmpty(options.SecretIv)
                    ? new AesCrypter(options.SecretKey)
                    : new AesWithIvCrypter(options.SecretKey, options.SecretIv);
                return new ConfigFileCrypter(new JsonConfigCrypter(aesCrypter), new ConfigFileCrypterOptions
                {
                    ReplaceCurrentConfig = options.Replace
                });
            }

            if (!string.IsNullOrEmpty(options.CertificatePath))
            {
                certLoader = new FilesystemCertificateLoader(options.CertificatePath, options.CertificatePassword);
            }
            else if (!string.IsNullOrEmpty(options.CertSubjectName))
            {
                certLoader = new StoreCertificateLoader(options.CertSubjectName);
            }

            var configCrypter = new JsonConfigCrypter(new RSACrypter(certLoader));

            var fileCrypter = new ConfigFileCrypter(configCrypter, new ConfigFileCrypterOptions()
            {
                ReplaceCurrentConfig = options.Replace
            });

            return fileCrypter;
        }
    }
}