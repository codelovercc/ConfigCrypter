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
                ? X509CertificateLoader.LoadCertificate(_rawData)
                : X509CertificateLoader.LoadPkcs12(_rawData, _password);
        }
    }
}