using System;
using System.Collections.Concurrent;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;

namespace SecureHttpClient.CertificatePinning
{
    internal class CertificatePinner
    {
        private readonly ConcurrentDictionary<string, string[]> _pins;
        private readonly ILogger _logger;

        public CertificatePinner(ILogger logger = null)
        {
            _pins = new ConcurrentDictionary<string, string[]>();
            _logger = logger;
        }

        public void AddPins(string hostname, string[] pins)
        {
            _logger?.LogDebug($"Add CertificatePinner: hostname:{hostname}, pins:{string.Join("|", pins)}");
            _pins[hostname] = pins; // Updates value if already existing
        }

        public bool HasPin(string hostname)
        {
            return _pins.ContainsKey(hostname);
        }

        public bool Check(string hostname, X509Certificate2 certificate)
        {
            // Get pins
            if (!_pins.TryGetValue(hostname, out var pins))
            {
                _logger?.LogDebug($"No certificate pin found for {hostname}");
                return true;
            }

            // Compute spki fingerprint
            var spkiFingerprint = SpkiFingerprint.Compute(certificate);

            // Check pin
            var match = Array.IndexOf(pins, spkiFingerprint) > -1;
            if (match)
            {
                _logger?.LogDebug($"Certificate pin is ok for {hostname}");
            }
            else
            {
                _logger?.LogInformation($"Certificate pin error for {hostname}: found {spkiFingerprint}, expected {string.Join("|", pins)}");
            }
            return match;
        }
    }
}
