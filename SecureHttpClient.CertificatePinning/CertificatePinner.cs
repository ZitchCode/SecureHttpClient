using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace SecureHttpClient.CertificatePinning
{
    internal class CertificatePinner
    {
        private readonly ConcurrentDictionary<string, string[]> _pins;

        public CertificatePinner()
        {
            _pins = new ConcurrentDictionary<string, string[]>();
        }

        public void AddPins(string hostname, string[] pins)
        {
            _pins[hostname] = pins; // Updates value if already existing
        }

        public bool HasPin(string hostname)
        {
            return _pins.ContainsKey(hostname);
        }

        public bool Check(string hostname, byte[] certificate)
        {
            // Get pins
            string[] pins;
            if (!_pins.TryGetValue(hostname, out pins))
            {
                Debug.WriteLine($"No certificate pin found for {hostname}");
                return true;
            }

            // Compute spki fingerprint
            var spkiFingerprint = SpkiFingerprint.Compute(certificate);

            // Check pin
            var match = Array.IndexOf(pins, spkiFingerprint) > -1;
            Debug.WriteLine(match ? $"Certificate pin is ok for {hostname}" : $"Certificate pin error for {hostname}: found {spkiFingerprint}, expected {string.Join("|", pins)}");
            return match;
        }
    }
}
