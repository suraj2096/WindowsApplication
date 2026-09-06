using EmailHub.Models;
using EmailHub.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EmailHub.ViewModels
{
    public class MainViewModel
    {
        private readonly AuthenticationService _authenticationService;
        private readonly GraphService _graphService;
        private readonly DatabaseService _databaseService;

        public ObservableCollection<EmailMessage> Emails
        { get; } = new();

        public string UserName { get; private set; } =
            "Not signed in";

        public string UserEmail { get; private set; } =
            string.Empty;

        public MainViewModel()
        {
            _authenticationService =
                new AuthenticationService();

            _graphService =
                new GraphService();

            _databaseService =
                new DatabaseService();
        }

        public async Task InitializeAsync()
        {
            await _databaseService.InitializeAsync();

            var cachedEmails =
                await _databaseService.GetEmailsAsync();

            foreach (var email in cachedEmails)
            {
                Emails.Add(email);
            }
        }

        public async Task LoginAndLoadEmailsAsync(CancellationToken cancellationToken)
        {
            var authenticationResult =
                await _authenticationService.LoginAsync(cancellationToken);

            if (authenticationResult == null)
            {
                throw new Exception(
                    "Login failed.");
            }

            UserName =
                authenticationResult.Account
                    .Username;

            UserEmail =
                authenticationResult.Account
                    .Username;

            var emails =
                await _graphService
                    .GetInboxMessagesAsync(
                        authenticationResult.AccessToken);

            await _databaseService
                .SaveEmailsAsync(emails);

            Emails.Clear();

            foreach (var email in emails)
            {
                Emails.Add(email);
            }
        }
    }
}
