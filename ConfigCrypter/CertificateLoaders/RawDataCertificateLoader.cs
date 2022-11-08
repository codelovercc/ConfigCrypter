using System.Security.Cryptography.X509Certificates;

namespace DevAttic.ConfigCrypter.CertificateLoaders
{
    public class RawDataCertificateLoader : ICertificateLoader
    {
        private readonly byte[] _rawData;
        private readonly string _password;

        public RawDataCertificateLoader(byte[] rawData, string password = null)
        {
            _password = password;
            _rawData = rawData;
        }

        public X509Certificate2 LoadCertificate()
        {
            return string.IsNullOrEmpty(_password)
                ? new X509Certificate2(_rawData)
                : new X509Certificate2(_rawData, _password);
        }
    }
}