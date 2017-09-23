using System;
namespace TempMail
{
    public class Email
    {
        public string MailId { get; set; }

        public string MailAddressId { get; set; }

        public string MailFrom { get; set; }

        public string MailSubject { get; set; }

        public string MailPreview { get; set; }

        public string MailTextOnly { get; set; }

        public string MailText { get; set; }

        public string MailHtml { get; set; }

        public DateTime MailTimestamp { get; set; }

        public string Error { get; set; }
    }
}
