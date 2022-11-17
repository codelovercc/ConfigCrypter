using CommandLine;

namespace ConfigCrypter.Console.Options
{
    [Verb("change", HelpText = "Change the secret key or certificate for the encrypted config file.")]
    public class ChangeEncryptionOptions : CommandlineOptions
    {
        [Option("path-new", Required = true, HelpText = "Path of the new certificate.", Group = "CertLocationNew")]
        public string CertificatePathNew { get; set; }

        [Option("name-new", Required = true,
            HelpText = "The subject name of the certificate (CN). This can only be used in Windows environments.",
            Group = "CertLocationNew")]
        public string CertSubjectNameNew { get; set; }

        [Option("password-new", Required = false, HelpText = "Password of the new certificate (if available).",
            Default = null)]
        public string CertificatePasswordNew { get; set; }

        [Option("secret-key-new", Required = true, HelpText = "Key for the symmetric encryption for change encryption.",
            Group = "CertLocationNew")]
        public string SecretKeyNew { get; set; }

        [Option("secret-iv-new", Required = false, HelpText = "Iv for the symmetric encryption for change encryption.")]
        public string SecretIvNew { get; set; }
    }
}