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

        [Fact]
        public void EncryptFile_DecryptFile_WithVerifyingFile_OnConsoleMainMethod()
        {
            var encryptArgs =
                $"encrypt -f {Config2OriginJsonFilePath} --secret-key 123456 --secret-iv   --key-prefix {KeyPrefix} -l JwtSettings.SecurityKey JwtSettings.ExpiresIn JwtSettings.Issuer JwtSettings.EncryptKey JwtSettings.RefreshTokenExpiresIn Logging.LogLevel['Microsoft.AspNetCore']";
            var args = encryptArgs.Split(' ');
            Program.Main(args);
            Assert.True(File.Exists(Config2EncryptedJsonFilePath));
            var decryptArgs =
                $"decrypt -f {Config2EncryptedJsonFilePath} --secret-key 123456 --secret-iv   --key-prefix {KeyPrefix} -l JwtSettings.SecurityKey JwtSettings.ExpiresIn JwtSettings.Issuer JwtSettings.EncryptKey JwtSettings.RefreshTokenExpiresIn Logging.LogLevel['Microsoft.AspNetCore']";
            args = decryptArgs.Split(' ');
            Program.Main(args);
            Assert.True(File.Exists(Config2DecryptedJsonFilePath));
            var originConfig = JObject.Parse(File.ReadAllText(Config2OriginJsonFilePath));
            var decryptedConfig = JObject.Parse(File.ReadAllText(Config2DecryptedJsonFilePath));
            foreach (var selectToken in originConfig.SelectTokens("$..*").Where(t => t is JValue).ToArray())
            {
                var parsedToken = decryptedConfig.SelectToken(selectToken.Path);
                Assert.NotNull(parsedToken);
                try
                {
                    var originValue = selectToken.Value<string>();
                    if (originValue.StartsWith(KeyPrefix))
                    {
                        originValue = originValue.Remove(0, KeyPrefix.Length);
                    }

                    var parsedValue = parsedToken.Value<string>();
                    Assert.Equal(originValue, parsedValue);
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
        //     var encryptArgs = "encrypt -f /Users/codelover/Documents/Project/ConfigCrypter/ConfigCrypter.Tests/appsettings_decrypted.json -k $.Test.ToBeEncrypted -p /Users/codelover/Documents/Project/ConfigCrypter/ConfigCrypter.Tests/test-certificate.pfx -s 123456";
        //     var args = encryptArgs.Split(' ');
        //     Program.Main(args);
        //     encryptArgs ="encrypt -f /Users/codelover/Documents/Project/ConfigCrypter/ConfigCrypter.Tests/config_decrypted.json -k $.KeyToEncrypt -p /Users/codelover/Documents/Project/ConfigCrypter/ConfigCrypter.Tests/test-certificate.pfx -s 123456";
        //     args = encryptArgs.Split(' ');
        //     Program.Main(args);
        // }
    }
}