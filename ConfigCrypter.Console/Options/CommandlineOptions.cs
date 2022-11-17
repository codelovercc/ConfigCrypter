using System.Collections.Generic;
using CommandLine;

namespace ConfigCrypter.Console.Options
{
    public class CommandlineOptions
    {
        [Option('p', "path", Required = true, HelpText = "Path of the certificate.", Group = "CertLocation")]
        public string CertificatePath { get; set; }

        [Option('n', "name", Required = true,
            HelpText = "The subject name of the certificate (CN). This can only be used in Windows environments.",
            Group = "CertLocation")]
        public string CertSubjectName { get; set; }

        [Option('s', "password", Required = false, HelpText = "Password of the certificate (if available).",
            Default = null)]
        public string CertificatePassword { get; set; }

        [Option("secret-key", Required = true, HelpText = "Key for the symmetric encryption.", Group = "CertLocation")]
        public string SecretKey { get; set; }

        [Option("secret-iv", Required = false, Default = null, HelpText = "Iv for the symmetric encryption.")]
        public string SecretIv { get; set; }

        [Option('k', "key", Group = "theKeys", Required = true,
            HelpText = "The key to encrypt in the config file, Json Path.")]
        public string Key { get; set; }

        [Option('l', "keys", Group = "theKeys", Required = true,
            HelpText = "The keys to encrypt in the config file, Json Path.")]
        public IEnumerable<string> Keys { get; set; }

        [Option("key-prefix", Group = "theKeys", Required = true,
            HelpText = "The prefix of the value of the keys to encrypt in the config file.")]
        public string KeyPrefix { get; set; }

        [Option('f', "file", Required = true, HelpText = "The path to the config file.")]
        public string ConfigFile { get; set; }

        [Option('r', "replace", HelpText = "Replaces the original file if passed as parameter.", Default = false)]
        public bool Replace { get; set; }

        [Option("format", Default = ConfigFormat.Json, HelpText = "The format of the config file.")]
        public ConfigFormat ConfigFormat { get; set; }
    }

    public enum ConfigFormat
    {
        Json
    }
}