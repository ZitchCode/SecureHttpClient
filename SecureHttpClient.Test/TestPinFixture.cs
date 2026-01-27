using System.Threading.Tasks;
using SecureHttpClient.CertificatePinning;
using Xunit;

namespace SecureHttpClient.Test
{
    public class TestPinFixture : TestFixture, IAsyncLifetime
    {
        public string Hostname => "www.howsmyssl.com";
        public string Wildcard1 => "*.howsmyssl.com";
        public string Wildcard2 => "**.howsmyssl.com";
        public string InvalidPattern => "*.*.howsmyssl.com";
        public string Page => "https://www.howsmyssl.com/a/check";
        public string[] PinsOk { get; private set; }

        public string Hostname2 => "github.com";
        public string Page2 => "https://github.com";
        public string[] Pins2Ok { get; private set; }

        public string Hostname3 => "ecc256.badssl.com";
        public string Page3 => "https://ecc256.badssl.com/";
        public string[] Pins3Ok { get; private set; }

        public string[] PinsKo => ["sha256/xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx="];

        public async Task InitializeAsync()
        {
            var certificate = await CertificateHelper.GetCertificateAsync(Hostname);
            var pin = CertificateHelper.GetSpkiFingerprint(certificate);
            PinsOk = [pin];

            var certificate2 = await CertificateHelper.GetCertificateAsync(Hostname2);
            var pin2 = CertificateHelper.GetSpkiFingerprint(certificate2);
            Pins2Ok = [pin2];

            var certificate3 = await CertificateHelper.GetCertificateAsync(Hostname3);
            var pin3 = CertificateHelper.GetSpkiFingerprint(certificate3);
            Pins3Ok = [pin3];
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
