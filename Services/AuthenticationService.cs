using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EmailHub.Services
{
    public class AuthenticationService
    {
        private const string ClientId =
            "add your client id";

        private readonly IPublicClientApplication _msalClient;

        private readonly string[] _scopes =
        {
            "User.Read",
            "Mail.Read"
        };

        public AuthenticationService()
        {
            _msalClient =
                PublicClientApplicationBuilder
                    .Create(ClientId)
                    .WithAuthority(
                        $"https://login.microsoftonline.com/common/")
                    .WithDefaultRedirectUri()
                    .Build();
        }

        public async Task<AuthenticationResult?> LoginAsync(
            CancellationToken cancellationToken)
        {
            try
            {
                return await _msalClient
                    .AcquireTokenInteractive(_scopes)
                    .WithPrompt(Prompt.SelectAccount)
                    .ExecuteAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // User clicked "Cancel and try again"
                return null;
            }
            catch (MsalException ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"MSAL Login error: {ex}");

                throw;
            }
        }
    }
}
