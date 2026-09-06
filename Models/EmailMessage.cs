using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailHub.Models
{
    public class EmailMessage
    {
        public string Id { get; set; } = string.Empty;

        public string SenderName { get; set; } = string.Empty;

        public string SenderEmail { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public string Preview { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public DateTimeOffset? ReceivedDateTime { get; set; }

        public bool IsRead { get; set; }

        public string Folder { get; set; } = "Inbox";
    }
}
