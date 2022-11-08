using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using DevAttic.ConfigCrypter.Crypters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace DevAttic.ConfigCrypter.ConfigProviders.Json
{
    /// <summary>
    ///  JSON configuration provider that uses the underlying crypter to decrypt the given keys.
    /// </summary>
    public class EncryptedJsonConfigProvider : JsonConfigurationProvider
    {
        private readonly EncryptedJsonConfigSource _jsonConfigSource;

        /// <summary>
        /// Creates an instance of the EncryptedJsonConfigProvider.
        /// </summary>
        /// <param name="source">EncryptedJsonConfigSource that is used to configure the provider.</param>
        public EncryptedJsonConfigProvider(EncryptedJsonConfigSource source) : base(source)
        {
            _jsonConfigSource = source;
        }

        /// <summary>
        /// Loads the JSON configuration from stream and decrypts all configured keys with the given crypter.
        /// </summary>
        public override void Load(Stream stream)
        {
            try
            {
                using (var crypter = _jsonConfigSource.CrypterFactory(_jsonConfigSource))
                {
                    Data = EncryptedJsonConfigurationFileParser.Parse(stream, crypter, _jsonConfigSource.KeysToDecrypt);
                }
            }
            catch (JsonException e)
            {
                throw new FormatException($"Error JSONParseError {e.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to decrypt keys", ex);
            }
        }
    }


    internal class EncryptedJsonConfigurationFileParser
    {
        private EncryptedJsonConfigurationFileParser()
        {
        }

        private readonly IDictionary<string, string> _data =
            new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private readonly Stack<string> _context = new Stack<string>();
        private string _currentPath;
        private ICrypter _crypter;
        private List<string> _keysToDecrypt;

        public static IDictionary<string, string> Parse(Stream input, ICrypter crypter, List<string> keysToDecrypt)
            => new EncryptedJsonConfigurationFileParser().ParseStream(input, crypter, keysToDecrypt);

        private IDictionary<string, string> ParseStream(Stream input, ICrypter crypter, List<string> keysToDecrypt)
        {
            _data.Clear();

            var jsonDocumentOptions = new JsonDocumentOptions
            {
                CommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
            };

            using (var reader = new StreamReader(input))
            using (var doc = JsonDocument.Parse(reader.ReadToEnd(), jsonDocumentOptions))
            {
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                {
                    throw new FormatException($"Error UnsupportedJSONToken {doc.RootElement.ValueKind}");
                }

                _crypter = crypter;
                _keysToDecrypt = keysToDecrypt;
                VisitElement(doc.RootElement);
            }

            return _data;
        }

        private void VisitElement(JsonElement element)
        {
            foreach (var property in element.EnumerateObject())
            {
                EnterContext(property.Name);
                VisitValue(property.Value);
                ExitContext();
            }
        }

        private void VisitValue(JsonElement value)
        {
            switch (value.ValueKind)
            {
                case JsonValueKind.Object:
                    VisitElement(value);
                    break;

                case JsonValueKind.Array:
                    var index = 0;
                    foreach (var arrayElement in value.EnumerateArray())
                    {
                        EnterContext(index.ToString());
                        VisitValue(arrayElement);
                        ExitContext();
                        index++;
                    }

                    break;

                case JsonValueKind.Number:
                case JsonValueKind.String:
                case JsonValueKind.True:
                case JsonValueKind.False:
                case JsonValueKind.Null:
                    var key = _currentPath;
                    if (_data.ContainsKey(key))
                    {
                        throw new FormatException($"Error KeyIsDuplicated {key}");
                    }

                    if (_keysToDecrypt.Contains(key))
                    {
                        _data[key] = _crypter.DecryptString(value.ToString());
                    }
                    else
                    {
                        _data[key] = value.ToString();
                    }

                    break;

                case JsonValueKind.Undefined:
                default:
                    throw new FormatException($"Error UnsupportedJSONToken {value.ValueKind}");
            }
        }

        private void EnterContext(string context)
        {
            _context.Push(context);
            _currentPath = ConfigurationPath.Combine(_context.Reverse());
        }

        private void ExitContext()
        {
            _context.Pop();
            _currentPath = ConfigurationPath.Combine(_context.Reverse());
        }
    }
}