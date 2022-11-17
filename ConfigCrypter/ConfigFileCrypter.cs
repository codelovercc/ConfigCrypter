using System.Collections.Generic;
using System.IO;
using DevAttic.ConfigCrypter.ConfigCrypters;

namespace DevAttic.ConfigCrypter
{
    /// <summary>
    /// Configuration crypter that reads the configuration file from the filesystem.
    /// </summary>
    public class ConfigFileCrypter
    {
        private readonly IConfigCrypter _configCrypter;
        private readonly ConfigFileCrypterOptions _options;

        /// <summary>
        /// Creates an instance of the ConfigFileCrypter.
        /// </summary>
        /// <param name="configCrypter">A config crypter instance.</param>
        /// <param name="options">Options used for encrypting and decrypting.</param>
        public ConfigFileCrypter(IConfigCrypter configCrypter, ConfigFileCrypterOptions options)
        {
            _configCrypter = configCrypter;
            _options = options;
        }

        /// <summary>
        /// Use new IConfigCrypter re-encrypt the encrypted json config file, and override the file content with new encrypted content.
        /// </summary>
        /// <param name="encryptedFilePath">The encrypted json config file path</param>
        /// <param name="configCrypter">The crypter with new secret key or certificate</param>
        /// <param name="keys">The keys(JsonPath) in the json config file that about to encrypt or decrypt</param>
        /// <param name="keyPrefix">The prefix of value that about to encrypt or decrypt</param>
        public void ReEncryptFile(string encryptedFilePath, IConfigCrypter configCrypter, List<string> keys = null,
            string keyPrefix = null)
        {
            var configContent = File.ReadAllText(encryptedFilePath);
            var decryptedConfigContent = _configCrypter.DecryptKeys(configContent, keys, keyPrefix, true);
            var encryptedConfigContent = configCrypter.EncryptKeys(decryptedConfigContent, keys, keyPrefix);

            var targetFilePath = GetDestinationConfigPath(encryptedFilePath, _options.ChangedConfigPostFix);
            File.WriteAllText(targetFilePath, encryptedConfigContent);
        }

        public void ReEncryptKeyInFile(string encryptedFilePath, IConfigCrypter configCrypter, string configKey)
        {
            var configContent = File.ReadAllText(encryptedFilePath);
            var decryptedConfigContent = _configCrypter.DecryptKey(configContent, configKey);
            var encryptedConfigContent = configCrypter.EncryptKey(decryptedConfigContent, configKey);

            var targetFilePath = GetDestinationConfigPath(encryptedFilePath, _options.ChangedConfigPostFix);
            File.WriteAllText(targetFilePath, encryptedConfigContent);
        }

        public void DecryptFile(string filePath, List<string> keys = null, string keyPrefix = null)
        {
            var configContent = File.ReadAllText(filePath);
            var decryptedConfigContent = _configCrypter.DecryptKeys(configContent, keys, keyPrefix);

            var targetFilePath = GetDestinationConfigPath(filePath, _options.DecryptedConfigPostfix);
            File.WriteAllText(targetFilePath, decryptedConfigContent);
        }

        /// <summary>
        /// <para>Decrypts the given key in the config file.</para>
        /// <para> </para>
        /// <para>If the "ReplaceCurrentConfig" setting has been set in the options the file is getting replaced.</para>
        /// <para>If the setting has not been set a new file with the "DecryptedConfigPostfix" appended to the current file name will be created.</para>
        /// </summary>
        /// <param name="filePath">Path of the configuration file.</param>
        /// <param name="configKey">Key to decrypt, passed in a format the underlying config crypter understands.</param>
        public void DecryptKeyInFile(string filePath, string configKey)
        {
            var configContent = File.ReadAllText(filePath);
            var decryptedConfigContent = _configCrypter.DecryptKey(configContent, configKey);

            var targetFilePath = GetDestinationConfigPath(filePath, _options.DecryptedConfigPostfix);
            File.WriteAllText(targetFilePath, decryptedConfigContent);
        }

        /// <summary>
        /// <para>Encrypts the given key in the config file.</para>
        /// <para> </para>
        /// <para>If the "ReplaceCurrentConfig" setting has been set in the options the file is getting replaced.</para>
        /// <para>If the setting has not been set a new file with the "EncryptedConfigPostfix" appended to the current file name will be created.</para>
        /// </summary>
        /// <param name="filePath">Path of the configuration file.</param>
        /// <param name="configKey">Key to encrypt, passed in a format the underlying config crypter understands.</param>
        public void EncryptKeyInFile(string filePath, string configKey)
        {
            var configContent = File.ReadAllText(filePath);
            var encryptedConfigContent = _configCrypter.EncryptKey(configContent, configKey);

            var targetFilePath = GetDestinationConfigPath(filePath, _options.EncryptedConfigPostfix);
            File.WriteAllText(targetFilePath, encryptedConfigContent);
        }

        public void EncryptFile(string filePath, List<string> keys = null, string keyPrefix = null)
        {
            var configContent = File.ReadAllText(filePath);
            var encryptedConfigContent = _configCrypter.EncryptKeys(configContent, keys, keyPrefix);

            var targetFilePath = GetDestinationConfigPath(filePath, _options.EncryptedConfigPostfix);
            File.WriteAllText(targetFilePath, encryptedConfigContent);
        }

        private string GetDestinationConfigPath(string currentConfigFilePath, string postfix)
        {
            if (_options.ReplaceCurrentConfig)
            {
                return currentConfigFilePath;
            }

            var currentConfigDirectory = Path.GetDirectoryName(currentConfigFilePath);
            var newFilename =
                $"{Path.GetFileNameWithoutExtension(currentConfigFilePath)}{postfix}{Path.GetExtension(currentConfigFilePath)}";
            var targetFile = Path.Combine(currentConfigDirectory, newFilename);

            return targetFile;
        }
    }
}