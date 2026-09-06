using EmailHub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EmailHub.Services
{
    public class GraphService
    {
        private readonly HttpClient _httpClient;

        public GraphService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<List<EmailMessage>> GetInboxMessagesAsync(
            string accessToken)
        {
            var emails = new List<EmailMessage>();

            var url =
                "https://graph.microsoft.com/v1.0/me/mailFolders/inbox/messages" +
                "?$top=25" +
                "&$select=id,subject,bodyPreview,body,from,receivedDateTime,isRead" +
                "&$orderby=receivedDateTime desc";

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                url);

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    accessToken);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                throw new Exception(
                    $"Graph API failed: {response.StatusCode}\n{error}");
            }

            var json = await response.Content.ReadAsStringAsync();

            using var document =
                JsonDocument.Parse(json);

            if (!document.RootElement.TryGetProperty(
                    "value",
                    out var messages))
            {
                return emails;
            }

            foreach (var message in messages.EnumerateArray())
            {
                var email = new EmailMessage
                {
                    Id = GetString(message, "id"),

                    Subject =
                        GetString(message, "subject"),

                    Preview =
                        GetString(message, "bodyPreview"),

                    Body =
                        GetBodyContent(message),

                    IsRead =
                        message.TryGetProperty(
                            "isRead",
                            out var isRead)
                        && isRead.GetBoolean(),

                    Folder = "Inbox"
                };

                if (message.TryGetProperty(
                        "receivedDateTime",
                        out var received))
                {
                    if (DateTimeOffset.TryParse(
                            received.GetString(),
                            out var date))
                    {
                        email.ReceivedDateTime = date;
                    }
                }

                if (message.TryGetProperty(
                        "from",
                        out var from))
                {
                    if (from.TryGetProperty(
                            "emailAddress",
                            out var emailAddress))
                    {
                        email.SenderName =
                            GetString(
                                emailAddress,
                                "name");

                        email.SenderEmail =
                            GetString(
                                emailAddress,
                                "address");
                    }
                }

                emails.Add(email);
            }

            return emails;
        }

        private static string GetString(
            JsonElement element,
            string property)
        {
            if (element.TryGetProperty(
                    property,
                    out var value))
            {
                return value.GetString() ?? string.Empty;
            }

            return string.Empty;
        }

        private static string GetBodyContent(
            JsonElement message)
        {
            if (!message.TryGetProperty(
                    "body",
                    out var body))
            {
                return string.Empty;
            }

            if (body.TryGetProperty(
                    "content",
                    out var content))
            {
                return content.GetString() ?? string.Empty;
            }

            return string.Empty;
        }
    }
}
