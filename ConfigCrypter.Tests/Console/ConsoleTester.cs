using System;
using System.IO;
using System.Linq;
using ConfigCrypter.Console;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DevAttic.ConfigCrypter.Tests.Console
{
    public class ConsoleTester
    {
        private const string Config2DecryptedJsonFilePath = "config2_encrypted_decrypted.json";
        private const string Config2EncryptedJsonFilePath = "config2_encrypted.json";
        private const string Config2OriginJsonFilePath = "config2.json";
        private const string KeyPrefix = "Encrypted_";
        private const string ChangingKeyFilePath = "config2_encrypted_forChangeSecretKey.json";
        private const string ChangedKeyFilePath = "config2_encrypted_forChangeSecretKey_changed.json";

        private const string SecretKey = "123456";

        [Fact]
        public void ChangeSecretKey_OnConsoleMainMethod()
        {
            const string listKeys =
                "JwtSettings.SecurityKey JwtSettings.ExpiresIn JwtSettings.Issuer JwtSettings.EncryptKey JwtSettings.RefreshTokenExpiresIn Logging.LogLevel['Microsoft.AspNetCore']";
            var changeArgs =
                $"change -f {ChangingKeyFilePath} --secret-key {SecretKey} --secret-iv   --secret-key-new 123123 --key-prefix {KeyPrefix} -l {listKeys}";
            var args = changeArgs.Split(' ');
            Program.Main(args);
            Assert.True(File.Exists(ChangedKeyFilePath));
            var listKeysArray = listKeys.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            AssertJsonValueNodesForTwoJsonConfigFile(ChangingKeyFilePath, ChangedKeyFilePath,
                (origin, current, originValue, currentValue) =>
                {
                    if (listKeysArray.Contains(origin.Path) || originValue.StartsWith(KeyPrefix))
                    {
                        Assert.NotEqual(originValue, currentValue);
                    }
                },
                removeKeyPrefixFromValue: false);
        }

        [Fact]
        public void EncryptFile_DecryptFile_WithVerifyingFile_OnConsoleMainMethod()
        {
            var encryptArgs =
                $"encrypt -f {Config2OriginJsonFilePath} --secret-key {SecretKey} --secret-iv   --key-prefix {KeyPrefix} -l JwtSettings.SecurityKey JwtSettings.ExpiresIn JwtSettings.Issuer JwtSettings.EncryptKey JwtSettings.RefreshTokenExpiresIn Logging.LogLevel['Microsoft.AspNetCore']";
            var args = encryptArgs.Split(' ');
            Program.Main(args);
            Assert.True(File.Exists(Config2EncryptedJsonFilePath));
            var decryptArgs =
                $"decrypt -f {Config2EncryptedJsonFilePath} --secret-key {SecretKey} --secret-iv   --key-prefix {KeyPrefix} -l JwtSettings.SecurityKey JwtSettings.ExpiresIn JwtSettings.Issuer JwtSettings.EncryptKey JwtSettings.RefreshTokenExpiresIn Logging.LogLevel['Microsoft.AspNetCore']";
            args = decryptArgs.Split(' ');
            Program.Main(args);
            Assert.True(File.Exists(Config2DecryptedJsonFilePath));

            AssertJsonValueNodesForTwoJsonConfigFile(Config2OriginJsonFilePath, Config2DecryptedJsonFilePath,
                (origin, current, originValue, currentValue) => Assert.Equal(originValue, currentValue));
        }

        /// <summary>
        /// Expands tow json config file and custom asserts for the json value token both in the config file
        /// </summary>
        /// <param name="originJsonFilePath"></param>
        /// <param name="currentJsonFilePath"></param>
        /// <param name="assertOriginAndCurrentJsonValueToken">first arg is the origin json value token, second arg is the current json value token. third arg is the value of the origin json token, fourth arg is the value of the current json token</param>
        /// <param name="assertJsonValueToken">first arg is the origin json value token, second arg is the current json value token. you can custom asserts for these two json tokens in this action</param>
        /// <param name="removeKeyPrefixFromValue">for the config value, defines the <see cref="KeyPrefix"/> should be removed in the value of origin json token</param>
        private static void AssertJsonValueNodesForTwoJsonConfigFile(string originJsonFilePath,
            string currentJsonFilePath,
            Action<JToken, JToken, string, string> assertOriginAndCurrentJsonValueToken,
            Action<JToken, JToken> assertJsonValueToken = null, bool removeKeyPrefixFromValue = true)
        {
            var originConfig = JObject.Parse(File.ReadAllText(originJsonFilePath));
            var currentConfig = JObject.Parse(File.ReadAllText(currentJsonFilePath));
            foreach (var selectToken in originConfig.SelectTokens("$..*").Where(t => t is JValue).ToArray())
            {
                var parsedToken = currentConfig.SelectToken(selectToken.Path);
                Assert.NotNull(parsedToken);
                assertJsonValueToken?.Invoke(selectToken, parsedToken);
                try
                {
                    var originValue = selectToken.Value<string>();
                    if (removeKeyPrefixFromValue && originValue.StartsWith(KeyPrefix))
                    {
                        originValue = originValue.Remove(0, KeyPrefix.Length);
                    }

                    var parsedValue = parsedToken.Value<string>();
                    assertOriginAndCurrentJsonValueToken(selectToken, parsedToken, originValue, parsedValue);
                }
                catch (InvalidCastException)
                {
                }
            }
        }

        // this code is for encrypt key when the certificate was changed.
        // [Fact]
        // public void EncryptKey_DecryptKey_WithVerifying_OnConsoleMainMethod()
        // {
        //     var encryptArgs = "encrypt -f ProjectPath/appsettings_decrypted.json -k $.Test.ToBeEncrypted -p ProjectPath/test-certificate.pfx -s 123456";
        //     var args = encryptArgs.Split(' ');
        //     Program.Main(args);
        //     encryptArgs ="encrypt -f ProjectPath/config_decrypted.json -k $.KeyToEncrypt -p ProjectPath/test-certificate.pfx -s 123456";
        //     args = encryptArgs.Split(' ');
        //     Program.Main(args);
        // }
    }
}