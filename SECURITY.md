# Security Policy

## Supported Versions

The following versions of SecureHttpClient are currently being supported with security updates:

| Version | Supported          | .NET Version | Platforms |
| ------- | ------------------ | ------------ | --------- |
| 2.4.x   | :white_check_mark: | .NET 10.0    | Android 8-16.1, iOS 26.1, Windows |
| 2.3.x   | :white_check_mark: | .NET 9.0     | Android, iOS, Windows |
| 2.2.x   | :x:                | .NET 8.0     | Android, iOS, Windows |
| 2.1.x   | :x:                | .NET 7.0     | Android, iOS, Windows |
| 2.0.x   | :x:                | .NET 6.0     | Android, iOS, Windows |
| 1.x     | :x:                | Legacy       | MonoAndroid, Xamarin.iOS, NetStandard |

We strongly recommend upgrading to the latest version to receive security updates and improvements.

## Security Features

SecureHttpClient provides several security features designed to enhance the security of HTTP communications:

### Certificate Pinning

Certificate pinning protects against man-in-the-middle attacks by validating that the server's certificate matches expected pins (SPKI fingerprints). This feature is critical for preventing certificate authority compromises and rogue certificates.

**Best Practices:**
- Pin intermediate or leaf certificates, not just root CAs
- Maintain backup pins for certificate rotation
- Use wildcard patterns (`**.example.com`) for flexibility
- Monitor certificate expiration dates
- Test pinning in development before production deployment

### TLS 1.2+ Enforcement

The library enforces TLS 1.2 or higher for all connections, protecting against protocol downgrade attacks and vulnerabilities in older TLS/SSL versions.

### Client Certificate Authentication

Support for client certificate authentication enables mutual TLS (mTLS) for enhanced authentication security.

### Custom Trusted Root CAs

Ability to specify custom trusted root certificate authorities for scenarios where private PKI is used.

## Reporting a Vulnerability

We take the security of SecureHttpClient seriously. If you believe you have found a security vulnerability, please report it to us as described below.

### Reporting Process

**Please do NOT report security vulnerabilities through public GitHub issues.**

Instead, please report them via email to: **contact@zitch.com**

Include the following information in your report:

1. **Description**: A clear description of the vulnerability
2. **Impact**: The potential impact and severity of the vulnerability
3. **Affected Versions**: Which versions of SecureHttpClient are affected
4. **Reproduction**: Step-by-step instructions to reproduce the issue
5. **Proof of Concept**: Code samples, screenshots, or other evidence (if available)
6. **Suggested Fix**: If you have ideas for how to fix the issue (optional)
7. **Your Information**: Your name/handle and contact information for follow-up

### What to Expect

After submitting a vulnerability report:

1. **Acknowledgment**: You should receive an acknowledgment within 48 hours
2. **Assessment**: We will assess the vulnerability and determine its severity and impact
3. **Updates**: We will keep you informed of our progress toward a fix
4. **Fix Development**: We will work on developing and testing a fix
5. **Disclosure**: We will coordinate with you on the timing of public disclosure
6. **Credit**: With your permission, we will credit you in the security advisory

### Response Timeline

- **Initial Response**: Within 48 hours
- **Status Update**: Within 7 days with assessment of severity
- **Fix Timeline**: Depends on severity
  - Critical: 7-14 days
  - High: 14-30 days
  - Medium: 30-60 days
  - Low: 60-90 days

## Security Best Practices for Users

When using SecureHttpClient in your applications:

### Certificate Pinning

1. **Always Enable for Sensitive Operations**: Enable certificate pinning for connections handling sensitive data

2. **Pin Multiple Certificates**: Include backup pins for certificate rotation
   ```csharp
   handler.AddCertificatePinner("api.example.com", new[] {
       "sha256/primaryKeyHash==",
       "sha256/backupKeyHash=="
   });
   ```

3. **Test Thoroughly**: Test pinning in development environments before production

4. **Monitor Certificate Expiration**: Set up monitoring for certificate expiration dates

5. **Have a Recovery Plan**: Plan for what happens if pinning fails (app update process)

### TLS Configuration

1. **Don't Bypass Certificate Validation**: Never disable certificate validation in production

2. **Use Strong Cipher Suites**: The library uses platform defaults which should be secure

3. **Keep Dependencies Updated**: Regularly update to the latest version of SecureHttpClient

### Client Certificates

1. **Secure Storage**: Store client certificates securely using platform keystore/keychain
   - Android: Use Android Keystore
   - iOS: Use iOS Keychain
   - Windows: Use Windows Certificate Store

2. **Protect Private Keys**: Never embed private keys in your application code

3. **Certificate Rotation**: Implement a process for rotating client certificates

### General Security

1. **Validate Server Responses**: Always validate server responses before processing

2. **Use HTTPS Only**: Never transmit sensitive data over unencrypted HTTP

3. **Implement Timeout Policies**: Set appropriate timeout values to prevent hanging connections

4. **Error Handling**: Don't expose sensitive information in error messages

5. **Logging**: Be careful not to log sensitive data (credentials, tokens, personal information)

## Known Security Considerations

### Platform-Specific Behavior

The library uses native platform implementations for TLS:
- **Android**: OkHttp3
- **iOS**: NSUrlSession
- **Windows/.NET**: HttpClientHandler

This means security characteristics may vary slightly between platforms based on the underlying OS and libraries.

### Certificate Pinning Limitations

- Pinning only validates the server certificate, not the entire chain
- Pinning can prevent users from connecting if certificates are rotated without an app update
- Pinning does not protect against compromised root CAs in the trusted store if pins are not configured

### Client Certificate Security

- Client certificates are only as secure as the platform's secure storage mechanism
- Root/jailbroken devices may have compromised secure storage

## Security Updates

Security updates will be announced through:

1. GitHub Security Advisories
2. Release notes in CHANGELOG.md
3. NuGet package release notes

Subscribe to GitHub repository notifications to stay informed about security updates.

## Security Audit History

No formal security audits have been conducted to date. The library is open source and community-reviewed.

## Responsible Disclosure

We follow responsible disclosure principles:

1. We will work with you to understand and validate the vulnerability
2. We will develop and test a fix before public disclosure
3. We will coordinate the timing of disclosure with you
4. We will credit you (with your permission) in the security advisory
5. We will not take legal action against researchers acting in good faith

## Additional Resources

- [OWASP Mobile Security Project](https://owasp.org/www-project-mobile-security/)
- [Certificate and Public Key Pinning](https://owasp.org/www-community/controls/Certificate_and_Public_Key_Pinning)
- [Transport Layer Protection Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Transport_Layer_Protection_Cheat_Sheet.html)

## Contact

For security-related inquiries: contact@zitch.com

For general questions: [GitHub Issues](https://github.com/ZitchCode/SecureHttpClient/issues)
