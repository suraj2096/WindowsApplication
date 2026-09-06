using EmailHub.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace EmailHub.Services
{
    public class DatabaseService
    {
        private readonly string _databasePath;

        public DatabaseService()
        {
            var folder =
                Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.LocalApplicationData),
                    "EmailHub");

            Directory.CreateDirectory(folder);

            _databasePath =
                Path.Combine(folder, "emailhub.db");
        }

        private SqliteConnection CreateConnection()
        {
            return new SqliteConnection(
                $"Data Source={_databasePath}");
        }

        public async Task InitializeAsync()
        {
            await using var connection =
                CreateConnection();

            await connection.OpenAsync();

            var command =
                connection.CreateCommand();

            command.CommandText =
            """
        CREATE TABLE IF NOT EXISTS Emails
        (
            Id TEXT PRIMARY KEY,
            SenderName TEXT,
            SenderEmail TEXT,
            Subject TEXT,
            Preview TEXT,
            Body TEXT,
            ReceivedDateTime TEXT,
            IsRead INTEGER,
            Folder TEXT
        );
        """;

            await command.ExecuteNonQueryAsync();
        }

        public async Task SaveEmailsAsync(
            IEnumerable<EmailMessage> emails)
        {
            await using var connection =
                CreateConnection();

            await connection.OpenAsync();

            foreach (var email in emails)
            {
                var command =
                    connection.CreateCommand();

                command.CommandText =
                """
            INSERT OR REPLACE INTO Emails
            (
                Id,
                SenderName,
                SenderEmail,
                Subject,
                Preview,
                Body,
                ReceivedDateTime,
                IsRead,
                Folder
            )
            VALUES
            (
                $id,
                $senderName,
                $senderEmail,
                $subject,
                $preview,
                $body,
                $receivedDateTime,
                $isRead,
                $folder
            );
            """;

                command.Parameters.AddWithValue(
                    "$id",
                    email.Id);

                command.Parameters.AddWithValue(
                    "$senderName",
                    email.SenderName);

                command.Parameters.AddWithValue(
                    "$senderEmail",
                    email.SenderEmail);

                command.Parameters.AddWithValue(
                    "$subject",
                    email.Subject);

                command.Parameters.AddWithValue(
                    "$preview",
                    email.Preview);

                command.Parameters.AddWithValue(
                    "$body",
                    email.Body);

                command.Parameters.AddWithValue(
                    "$receivedDateTime",
                    email.ReceivedDateTime?
                        .ToString("O")
                        ?? string.Empty);

                command.Parameters.AddWithValue(
                    "$isRead",
                    email.IsRead ? 1 : 0);

                command.Parameters.AddWithValue(
                    "$folder",
                    email.Folder);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<List<EmailMessage>>
            GetEmailsAsync()
        {
            var emails =
                new List<EmailMessage>();

            await using var connection =
                CreateConnection();

            await connection.OpenAsync();

            var command =
                connection.CreateCommand();

            command.CommandText =
            """
        SELECT
            Id,
            SenderName,
            SenderEmail,
            Subject,
            Preview,
            Body,
            ReceivedDateTime,
            IsRead,
            Folder
        FROM Emails
        ORDER BY ReceivedDateTime DESC;
        """;

            await using var reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var email =
                    new EmailMessage
                    {
                        Id =
                            reader.GetString(0),

                        SenderName =
                            reader.GetString(1),

                        SenderEmail =
                            reader.GetString(2),

                        Subject =
                            reader.GetString(3),

                        Preview =
                            reader.GetString(4),

                        Body =
                            reader.GetString(5),

                        IsRead =
                            reader.GetInt32(7) == 1,

                        Folder =
                            reader.GetString(8)
                    };

                if (DateTimeOffset.TryParse(
                        reader.GetString(6),
                        out var date))
                {
                    email.ReceivedDateTime = date;
                }

                emails.Add(email);
            }

            return emails;
        }
    }
}
