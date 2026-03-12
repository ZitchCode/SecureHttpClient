#if __IOS__

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Foundation;
using SecureHttpClient.CertificatePinning;

namespace SecureHttpClient
{
    internal class DataTaskDelegate : NSUrlSessionDataDelegate
    {
        private readonly SecureHttpClientHandler _secureHttpClientHandler;
        private readonly CertificatePinner _certificatePinner;
        private readonly X509Certificate2Collection _trustedRoots;

        public DataTaskDelegate(SecureHttpClientHandler secureHttpClientHandler, CertificatePinner certificatePinner, X509Certificate2Collection trustedRoots)
        {
            _secureHttpClientHandler = secureHttpClientHandler;
            _certificatePinner = certificatePinner;
            _trustedRoots = trustedRoots;
        }

        public override void DidReceiveResponse(NSUrlSession session, NSUrlSessionDataTask dataTask, NSUrlResponse response, Action<NSUrlSessionResponseDisposition> completionHandler)
        {
            var data = GetResponseForTask(dataTask);

            try
            {
                if (data.CancellationToken.IsCancellationRequested)
                {
                    dataTask.Cancel();
                }

                var resp = (NSHttpUrlResponse)response;

                var content = new CancellableStreamContent(data.ResponseBody, () =>
                {
                    if (!data.IsCompleted)
                    {
                        dataTask.Cancel();
                    }
                    data.IsCompleted = true;

                    data.ResponseBody.SetException(new OperationCanceledException());
                })
                {
                    Progress = data.Progress
                };


                // NB: The double cast is because of a Xamarin compiler bug
                var status = (int)resp.StatusCode;
                var ret = new HttpResponseMessage((HttpStatusCode)status)
                {
                    Content = content,
                    RequestMessage = data.Request,
                };
                ret.RequestMessage.RequestUri = new Uri(resp.Url.AbsoluteString);

                foreach (var v in resp.AllHeaderFields)
                {
                    // NB: Cocoa trolling us so hard by giving us back dummy dictionary entries
                    if (v.Key == null || v.Value == null) continue;

                    var headerKey = v.Key.ToString();

                    if (headerKey.ToLower() == "set-cookie")
                    {
                        var splitter = new SetCookieHeaderSplitter(v.Value.ToString());
                        while (splitter.HasNext())
                        {
                            var setCookieHeaderValue = splitter.Next();
                            ret.Headers.TryAddWithoutValidation(headerKey, setCookieHeaderValue);
                            ret.Content.Headers.TryAddWithoutValidation(headerKey, setCookieHeaderValue);
                        }
                    }
                    else
                    {
                        ret.Headers.TryAddWithoutValidation(headerKey, v.Value.ToString());
                        ret.Content.Headers.TryAddWithoutValidation(headerKey, v.Value.ToString());
                    }
                }

                data.FutureResponse.TrySetResult(ret);
            }
            catch (Exception ex)
            {
                data.FutureResponse.TrySetException(ex);
            }

            completionHandler(NSUrlSessionResponseDisposition.Allow);
        }

        public override void WillCacheResponse(NSUrlSession session, NSUrlSessionDataTask dataTask, NSCachedUrlResponse proposedResponse, Action<NSCachedUrlResponse> completionHandler)
        {
            completionHandler(proposedResponse);
        }

        public override void DidCompleteWithError(NSUrlSession session, NSUrlSessionTask task, NSError error)
        {
            var data = GetResponseForTask(task);
            data.IsCompleted = true;

            if (error != null || data.Error != null)
            {
                var ex = CreateExceptionForNsError(data.Error ?? error);

                // Pass the exception to the response
                data.FutureResponse.TrySetException(ex);
                data.ResponseBody.SetException(ex);
                return;
            }

            data.ResponseBody.Complete();

            lock (_secureHttpClientHandler.InflightRequests)
            {
                _secureHttpClientHandler.InflightRequests.Remove(task);
            }
        }

        public override void DidReceiveData(NSUrlSession session, NSUrlSessionDataTask dataTask, NSData byteData)
        {
            var data = GetResponseForTask(dataTask);
            var bytes = byteData.ToArray();

            // NB: If we're cancelled, we still might have one more chunk of data that attempts to be delivered
            if (data.IsCompleted) return;

            data.ResponseBody.AddByteArray(bytes);
        }

        private InflightOperation GetResponseForTask(NSUrlSessionTask task)
        {
            lock (_secureHttpClientHandler.InflightRequests)
            {
                return _secureHttpClientHandler.InflightRequests[task];
            }
        }

        public override void DidReceiveChallenge(NSUrlSession session, NSUrlSessionTask task, NSUrlAuthenticationChallenge challenge, Action<NSUrlSessionAuthChallengeDisposition, NSUrlCredential> completionHandler)
        {
            if (challenge.ProtectionSpace.AuthenticationMethod == NSUrlProtectionSpace.AuthenticationMethodNTLM)
            {
                if (_secureHttpClientHandler.Credentials != null)
                {
                    NetworkCredential credentialsToUse;
                    if (_secureHttpClientHandler.Credentials is NetworkCredential credentials)
                    {
                        credentialsToUse = credentials;
                    }
                    else
                    {
                        var uri = GetResponseForTask(task).Request.RequestUri;
                        credentialsToUse = _secureHttpClientHandler.Credentials.GetCredential(uri, "NTLM");
                    }
                    var credential = new NSUrlCredential(credentialsToUse.UserName, credentialsToUse.Password, NSUrlCredentialPersistence.ForSession);
                    completionHandler(NSUrlSessionAuthChallengeDisposition.UseCredential, credential);
                    return;
                }
            }

            if (challenge.ProtectionSpace.AuthenticationMethod == NSUrlProtectionSpace.AuthenticationMethodServerTrust)
            {
                var serverTrust = challenge.ProtectionSpace.ServerSecTrust;
                var hostname = task.CurrentRequest?.Url?.Host ?? challenge.ProtectionSpace.Host;
                var hasTrustedRoots = _trustedRoots != null && _trustedRoots.Count > 0;
                var hasPin = _certificatePinner != null && !string.IsNullOrEmpty(hostname) && _certificatePinner.HasPin(hostname);

                if (hasTrustedRoots)
                {
                    // Use .NET X509Chain instead of iOS SecTrust SetAnchorCertificates, which is unreliable
                    // on iOS 16+ with the .NET bindings.
                    var serverNativeChain = serverTrust.GetCertificateChain();
                    if (serverNativeChain == null || serverNativeChain.Length == 0)
                    {
                        RejectServerCertificate(task, completionHandler);
                        return;
                    }

                    var leafX509 = serverNativeChain[0].ToX509Certificate2();
                    if (leafX509 == null)
                    {
                        RejectServerCertificate(task, completionHandler);
                        return;
                    }

                    using var x509chain = new X509Chain();
                    x509chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                    x509chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
                    // Populate the discovery store with server-provided intermediates
                    for (int i = 1; i < serverNativeChain.Length; i++)
                    {
                        var cert = serverNativeChain[i].ToX509Certificate2();
                        if (cert != null) x509chain.ChainPolicy.ExtraStore.Add(cert);
                    }
                    // Add our trusted roots so the chain builder can resolve to them
                    foreach (X509Certificate2 trustedRoot in _trustedRoots)
                        x509chain.ChainPolicy.ExtraStore.Add(trustedRoot);

                    // Build returns false when root is not in system store; that's expected for custom CAs
                    var chainBuilt = x509chain.Build(leafX509);
                    if (!chainBuilt && x509chain.ChainStatus.Any(s => s.Status != X509ChainStatusFlags.UntrustedRoot))
                    {
                        RejectServerCertificate(task, completionHandler);
                        return;
                    }

                    // Verify the resolved chain root is actually one of our explicitly trusted roots
                    var chainRoot = x509chain.ChainElements[x509chain.ChainElements.Count - 1].Certificate;
                    var trustedRootFound = _trustedRoots.Cast<X509Certificate2>()
                        .Any(r => r.Thumbprint.Equals(chainRoot.Thumbprint, StringComparison.OrdinalIgnoreCase));
                    if (!trustedRootFound)
                    {
                        RejectServerCertificate(task, completionHandler);
                        return;
                    }

                    if (hasPin && !_certificatePinner.Check(hostname, leafX509))
                    {
                        RejectServerCertificate(task, completionHandler);
                        return;
                    }

                    completionHandler(NSUrlSessionAuthChallengeDisposition.UseCredential, NSUrlCredential.FromTrust(serverTrust));
                    return;
                }

                // No custom roots — pin-only validation using system SecTrust
                if (hasPin)
                {
                    var status = serverTrust.Evaluate(out _);
                    if (!status)
                    {
                        RejectServerCertificate(task, completionHandler);
                        return;
                    }

                    var serverChain = serverTrust.GetCertificateChain();
                    if (serverChain == null || serverChain.Length == 0)
                    {
                        RejectServerCertificate(task, completionHandler);
                        return;
                    }

                    var x509Certificate = serverChain[0].ToX509Certificate2();
                    if (!_certificatePinner.Check(hostname, x509Certificate))
                    {
                        RejectServerCertificate(task, completionHandler);
                        return;
                    }

                    completionHandler(NSUrlSessionAuthChallengeDisposition.UseCredential, NSUrlCredential.FromTrust(serverTrust));
                    return;
                }
            }

            if (challenge.ProtectionSpace.AuthenticationMethod == NSUrlProtectionSpace.AuthenticationMethodClientCertificate)
            {
                var certificate = _secureHttpClientHandler.ClientCertificate;
                if (certificate == null)
                {
                    var url = task.CurrentRequest.Url;
                    var space = new NSUrlProtectionSpace(url.Host, url.Port, url.Scheme, null, NSUrlProtectionSpace.AuthenticationMethodClientCertificate);
                    certificate = NSUrlCredentialStorage.SharedCredentialStorage.GetDefaultCredential(space);
                }
                if (certificate != null)
                {
                    completionHandler(NSUrlSessionAuthChallengeDisposition.UseCredential, certificate);
                    return;
                }
            }

            completionHandler(NSUrlSessionAuthChallengeDisposition.PerformDefaultHandling, challenge.ProposedCredential);
        }

        private void RejectServerCertificate(NSUrlSessionTask task, Action<NSUrlSessionAuthChallengeDisposition, NSUrlCredential> completionHandler)
        {
            var inflightRequest = GetResponseForTask(task);
            inflightRequest.Error = new NSError(NSError.NSUrlErrorDomain, (nint)(long)NSUrlError.ServerCertificateUntrusted);
            completionHandler(NSUrlSessionAuthChallengeDisposition.CancelAuthenticationChallenge, null);
        }

        public override void WillPerformHttpRedirection(NSUrlSession session, NSUrlSessionTask task, NSHttpUrlResponse response, NSUrlRequest newRequest, Action<NSUrlRequest> completionHandler)
        {
            var nextRequest = _secureHttpClientHandler.AllowAutoRedirect ? newRequest : null;
            completionHandler(nextRequest);
        }

        private static Exception CreateExceptionForNsError(NSError error)
        {
            var innerException = new NSErrorException(error);

            if ((error.Domain == NSError.NSUrlErrorDomain) || (error.Domain == NSError.CFNetworkErrorDomain))
            {
                switch ((NSUrlError)(long)error.Code)
                {
                    case NSUrlError.Cancelled:
                    case NSUrlError.UserCancelledAuthentication:
                    case (NSUrlError)NSNetServicesStatus.CancelledError:
                        return new OperationCanceledException(error.LocalizedDescription, innerException);

                    case NSUrlError.SecureConnectionFailed:
                    case NSUrlError.ServerCertificateHasBadDate:
                    case NSUrlError.ServerCertificateHasUnknownRoot:
                    case NSUrlError.ServerCertificateNotYetValid:
                    case NSUrlError.ServerCertificateUntrusted:
                    case NSUrlError.ClientCertificateRejected:
                    case NSUrlError.ClientCertificateRequired:
                        return new HttpRequestException(error.LocalizedDescription, 
                            new AuthenticationException(error.LocalizedDescription, innerException));
                }
            }

            return new HttpRequestException(error.LocalizedDescription, innerException);
        }
    }
}

#endif