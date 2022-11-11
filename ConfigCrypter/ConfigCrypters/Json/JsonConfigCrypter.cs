using System;
using System.Collections.Generic;
using System.Linq;
using DevAttic.ConfigCrypter.Crypters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DevAttic.ConfigCrypter.ConfigCrypters.Json
{
    /// <summary>
    /// Config crypter that encrypts and decrypts keys in JSON config files.
    /// </summary>
    public class JsonConfigCrypter : IConfigCrypter
    {
        private readonly ICrypter _crypter;

        /// <summary>
        /// Creates an instance of the JsonConfigCrypter.
        /// </summary>
        /// <param name="crypter">An ICrypter instance.</param>
        public JsonConfigCrypter(ICrypter crypter)
        {
            _crypter = crypter;
        }

        /// <summary>
        /// Decrypts the key in the given content of a config file.
        /// </summary>
        /// <param name="configFileContent">String content of a config file.</param>
        /// <param name="configKey">Key of the config entry. The key has to be in JSONPath format.</param>
        /// <returns>The content of the config file where the key has been decrypted.</returns>
        public string DecryptKey(string configFileContent, string configKey)
        {
            var (parsedConfig, settingsToken) = ParseConfig(configFileContent, configKey);

            var encryptedValue = _crypter.DecryptString(settingsToken.Value<string>());
            settingsToken.Replace(encryptedValue);
            var newConfigContent = parsedConfig.ToString(Formatting.Indented);

            return newConfigContent;
        }

        public string DecryptKeys(string configFileContent, List<string> configKeys, string keyPrefix)
        {
            JObject parsedConfig = null;
            if (configKeys?.Any() == true)
            {
                parsedConfig = EditConfig(JObject.Parse(configFileContent), configKeys, s => _crypter.DecryptString(s));
            }

            if (!string.IsNullOrEmpty(keyPrefix))
            {
                if (parsedConfig == null)
                {
                    parsedConfig = JObject.Parse(configFileContent);
                }

                parsedConfig = EditConfig(parsedConfig, keyPrefix,
                    s => _crypter.DecryptString(s.Remove(0, keyPrefix.Length)));
            }

            if (parsedConfig == null)
            {
                throw new InvalidOperationException("List of keys and key prefix can not be both empty or null");
            }

            return parsedConfig.ToString(Formatting.Indented);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Encrypts the key in the given content of a config file.
        /// </summary>
        /// <param name="configFileContent">String content of a config file.</param>
        /// <param name="configKey">Key of the config entry. The key has to be in JSONPath format.</param>
        /// <returns>The content of the config file where the key has been encrypted.</returns>
        public string EncryptKey(string configFileContent, string configKey)
        {
            var (parsedConfig, settingsToken) = ParseConfig(configFileContent, configKey);

            var encryptedValue = _crypter.EncryptString(settingsToken.Value<string>());
            settingsToken.Replace(encryptedValue);
            var newConfigContent = parsedConfig.ToString(Formatting.Indented);

            return newConfigContent;
        }

        public string EncryptKeys(string configFileContent, List<string> configKeys, string keyPrefix)
        {
            JObject parsedConfig = null;
            if (configKeys?.Any() == true)
            {
                parsedConfig = EditConfig(JObject.Parse(configFileContent), configKeys, s => _crypter.EncryptString(s));
            }

            if (!string.IsNullOrEmpty(keyPrefix))
            {
                if (parsedConfig == null)
                {
                    parsedConfig = JObject.Parse(configFileContent);
                }

                parsedConfig = EditConfig(parsedConfig, keyPrefix,
                    s => keyPrefix + _crypter.EncryptString(s.Remove(0, keyPrefix.Length)));
            }

            if (parsedConfig == null)
            {
                throw new InvalidOperationException("List of keys and key prefix can not be both empty or null");
            }

            return parsedConfig.ToString(Formatting.Indented);
        }

        private static JObject EditConfig(JObject parsedConfig, List<string> configKeys,
            Func<string, string> valueEdit)
        {
            foreach (var configKey in configKeys)
            {
                var t = parsedConfig.SelectToken(configKey);
                if (t == null)
                {
                    throw new InvalidOperationException($"The key {configKey} could not be found.");
                }

                var encryptedValue = valueEdit(t.Value<string>());
                t.Replace(encryptedValue);
            }

            return parsedConfig;
        }

        private static JObject EditConfig(JObject parsedConfig, string keyPrefix, Func<string, string> valueEdit)
        {
            var tokens = parsedConfig.SelectTokens("$..*").Where(t => t is JValue).ToArray();
            foreach (var token in tokens)
            {
                try
                {
                    var s = token.Value<string>();
                    if (s.StartsWith(keyPrefix))
                    {
                        token.Replace(valueEdit(s));
                    }
                }
                catch (InvalidCastException)
                {
                }
            }

            return parsedConfig;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _crypter?.Dispose();
            }
        }

        private (JObject ParsedConfig, JToken Key) ParseConfig(string json, string configKey)
        {
            var parsedJson = JObject.Parse(json);
            var keyToken = parsedJson.SelectToken(configKey);

            if (keyToken == null)
            {
                throw new InvalidOperationException($"The key {configKey} could not be found.");
            }

            return (parsedJson, keyToken);
        }
    }
}