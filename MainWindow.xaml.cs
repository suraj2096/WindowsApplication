using EmailHub.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EmailHub
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; }
        private CancellationTokenSource? _loginCancellationTokenSource;

        private bool _initialized;

        public MainWindow()
        {
            this.InitializeComponent();

            ViewModel = new MainViewModel();

            // Start with login screen
            LoginView.Visibility = Visibility.Visible;
            MailView.Visibility = Visibility.Collapsed;

            this.Activated += MainWindow_Activated;
        }


        private async void MainWindow_Activated(
            object sender,
            WindowActivatedEventArgs e)
        {
            if (_initialized)
                return;

            _initialized = true;

            // IMPORTANT:
            // Do NOT automatically login here.
            //
            // The user should first see the Login screen
            // and click "Sign in with Microsoft".

            await Task.CompletedTask;
        }


        private async void LoginButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                _loginCancellationTokenSource?.Dispose();

                _loginCancellationTokenSource =
                    new CancellationTokenSource();

                // Update UI
                LoginButton.IsEnabled = false;
                LoginButton.Content = "Signing in...";

                LoginProgress.Visibility = Visibility.Visible;
                LoginProgress.IsActive = true;

                CancelLoginButton.Visibility = Visibility.Visible;

                // Start authentication
                await ViewModel.LoginAndLoadEmailsAsync(
                    _loginCancellationTokenSource.Token);

                // Login succeeded
                LoginProgress.IsActive = false;
                LoginProgress.Visibility = Visibility.Collapsed;

                CancelLoginButton.Visibility = Visibility.Collapsed;

                LoginView.Visibility = Visibility.Collapsed;
                MailView.Visibility = Visibility.Visible;
            }
            catch (OperationCanceledException)
            {
                ResetLoginUI();
            }
            catch (Exception ex)
            {
                ResetLoginUI();

                var dialog =
                    new Microsoft.UI.Xaml.Controls.ContentDialog
                    {
                        Title = "Login failed",
                        Content = ex.Message,
                        CloseButtonText = "OK",
                        XamlRoot = Content.XamlRoot
                    };

                await dialog.ShowAsync();
            }
            finally
            {
                _loginCancellationTokenSource?.Dispose();
                _loginCancellationTokenSource = null;
            }
        }


        private void CancelLoginButton_Click(
    object sender,
    RoutedEventArgs e)
        {
            _loginCancellationTokenSource?.Cancel();

            ResetLoginUI();
        }

        private void ResetLoginUI()
        {
            LoginProgress.IsActive = false;
            LoginProgress.Visibility = Visibility.Collapsed;

            CancelLoginButton.Visibility = Visibility.Collapsed;

            LoginButton.IsEnabled = true;
            LoginButton.Content = "Sign in with Microsoft";
        }

        private async Task ShowLoginErrorAsync(
    string title,
    string message)
        {
            var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = Content.XamlRoot
            };

            await dialog.ShowAsync();
        }


        private void SignOutButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            LoginView.Visibility = Visibility.Visible;
            MailView.Visibility = Visibility.Collapsed;
        }


        private void EmailList_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (EmailList.SelectedItem is Models.EmailMessage email)
            {
                EmailSubjectText.Text =
                    email.Subject ?? "(No subject)";

                EmailSenderText.Text =
                    $"{email.SenderName} <{email.SenderEmail}>";

                EmailBodyText.Text =
                    email.Body ?? email.Preview ?? "";
            }
        }
    }
}
