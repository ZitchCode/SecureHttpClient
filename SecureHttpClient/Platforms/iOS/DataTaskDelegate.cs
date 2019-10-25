using System;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using Foundation;
using SecureHttpClient.CertificatePinning;
using Security;
using System.Security.Cryptography.X509Certificates;

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

                    ret.Headers.TryAddWithoutValidation(v.Key.ToString(), v.Value.ToString());
                    ret.Content.Headers.TryAddWithoutValidation(v.Key.ToString(), v.Value.ToString());
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
                    var credentials = _secureHttpClientHandler.Credentials as NetworkCredential;
                    if (credentials != null)
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
                challenge.ProtectionSpace.ServerSecTrust.SetAnchorCertificates(_trustedRoots);

                var hostname = task.CurrentRequest.Url.Host;
                if (_certificatePinner != null && _certificatePinner.HasPin(hostname))
                {
                    var serverTrust = challenge.ProtectionSpace.ServerSecTrust;
                    var status = serverTrust.Evaluate();
                    if (status == SecTrustResult.Proceed || status == SecTrustResult.Unspecified)
                    {
                        var serverCertificate = serverTrust[0];
                        var x509Certificate = serverCertificate.ToX509Certificate2();
                        var match = _certificatePinner.Check(hostname, x509Certificate.RawData);
                        if (match)
                        {
                            completionHandler(NSUrlSessionAuthChallengeDisposition.UseCredential, NSUrlCredential.FromTrust(serverTrust));
                        }
                        else
                        {
                            var inflightRequest = GetResponseForTask(task);
                            inflightRequest.Error = new NSError(NSError.NSUrlErrorDomain, (nint)(long)NSUrlError.ServerCertificateUntrusted);
                            completionHandler(NSUrlSessionAuthChallengeDisposition.CancelAuthenticationChallenge, null);
                        }
                        return;
                    }
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
