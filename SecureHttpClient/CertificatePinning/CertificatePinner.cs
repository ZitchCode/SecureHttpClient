using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

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
            ValidatePattern(hostname);
            _pins[hostname] = pins; // Updates value if already existing
        }

        public bool HasPin(string hostname)
        {
            if (string.IsNullOrEmpty(hostname))
            {
                throw new ArgumentException("hostname cannot be null or empty");
            }
            foreach (var (pattern, pins) in _pins)
            {
                if (MatchesPattern(pattern, hostname))
                {
                    return true;
                }
            }
            return false;
        }

        public bool Check(string hostname, X509Certificate2 certificate)
        {
            // Get matching pins
            var pins = GetMatchingPins(hostname);
            if (pins.Length == 0)
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

        internal static void ValidatePattern(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
            {
                throw new ArgumentException("Pattern cannot be null or empty");
            }
            if (pattern.StartsWith("*."))
            {
                if (pattern.IndexOf('*', 1) != -1)
                {
                    throw new ArgumentException($"Unexpected pattern: {pattern}");
                }
            }
            else if (pattern.StartsWith("**."))
            {
                if (pattern.IndexOf('*', 2) != -1)
                {
                    throw new ArgumentException($"Unexpected pattern: {pattern}");
                }
            }
            else if (pattern.Contains('*'))
            {
                throw new ArgumentException($"Unexpected pattern: {pattern}");
            }
        }


        private string[] GetMatchingPins(string hostname)
        {
            if (string.IsNullOrEmpty(hostname))
            {
                throw new ArgumentException("hostname cannot be null or empty");
            }
            var matchedPins = new HashSet<string>();
            foreach (var (pattern, pins) in _pins)
            {
                if (MatchesPattern(pattern, hostname))
                {
                    foreach (var pin in pins)
                    {
                        matchedPins.Add(pin);
                    }
                }
            }
            return matchedPins.ToArray();
        }

        internal static bool MatchesPattern(string pattern, string hostname)
        {
            if (pattern.StartsWith("**."))
            {
                var suffix = pattern[3..];
                return hostname == suffix || hostname.EndsWith("." + suffix);
            }
            if (pattern.StartsWith("*."))
            {
                var suffix = pattern[2..];
                if (!hostname.EndsWith("." + suffix))
                {
                    return false;
                }
                var prefix = hostname[..(hostname.Length - suffix.Length - 1)];
                return !prefix.Contains('.');
            }
            return hostname == pattern;
        }
    }
}
