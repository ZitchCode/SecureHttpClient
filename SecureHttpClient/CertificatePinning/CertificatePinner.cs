using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace SecureHttpClient.CertificatePinning
{
    /// <summary>
    /// Manages certificate pinning for hostnames by validating server certificates
    /// against configured SPKI fingerprints.
    /// </summary>
    internal class CertificatePinner
    {
        private readonly ConcurrentDictionary<string, string[]> _pins;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the CertificatePinner class.
        /// </summary>
        /// <param name="logger">Optional logger for diagnostic messages.</param>
        public CertificatePinner(ILogger logger = null)
        {
            _pins = new ConcurrentDictionary<string, string[]>();
            _logger = logger;
        }

        /// <summary>
        /// Adds certificate pins for a hostname pattern.
        /// </summary>
        /// <param name="hostname">The hostname or pattern (e.g., "example.com", "*.example.com", "**.example.com").</param>
        /// <param name="pins">Array of SPKI fingerprints in the format "sha256/&lt;base64-hash&gt;".</param>
        /// <exception cref="ArgumentException">Thrown when the hostname pattern is invalid.</exception>
        public void AddPins(string hostname, string[] pins)
        {
            _logger?.LogDebug($"Add CertificatePinner: hostname:{hostname}, pins:{string.Join("|", pins)}");
            ValidatePattern(hostname);
            _pins[hostname] = pins; // Updates value if already existing
        }

        /// <summary>
        /// Checks if any certificate pins are configured for the given hostname.
        /// </summary>
        /// <param name="hostname">The hostname to check.</param>
        /// <returns>True if pins are configured for the hostname; otherwise, false.</returns>
        /// <exception cref="ArgumentException">Thrown when hostname is null or empty.</exception>
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

        /// <summary>
        /// Validates a certificate against configured pins for the given hostname.
        /// </summary>
        /// <param name="hostname">The hostname to validate.</param>
        /// <param name="certificate">The certificate to check.</param>
        /// <returns>True if no pins are configured for the hostname or if the certificate matches a configured pin; otherwise, false.</returns>
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

        /// <summary>
        /// Validates that a hostname pattern is in a supported format.
        /// Supported patterns: exact hostname, "*.domain", or "**.domain".
        /// </summary>
        /// <param name="pattern">The hostname pattern to validate.</param>
        /// <exception cref="ArgumentException">Thrown when the pattern is invalid or unsupported.</exception>
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

        /// <summary>
        /// Determines if a hostname matches a given pattern.
        /// </summary>
        /// <param name="pattern">The pattern to match against (exact, "*.domain", or "**.domain").</param>
        /// <param name="hostname">The hostname to check.</param>
        /// <returns>True if the hostname matches the pattern; otherwise, false.</returns>
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
