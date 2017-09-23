using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using DnsClient;
using FluentEmail.Core;
using FluentEmail.Smtp;
using NUnit.Framework;

using Attachment = FluentEmail.Core.Models.Attachment;

namespace TempMailApi.Tests
{
    [TestFixture()]
    public class Test
    {
        private const string SENDER = "john@email.com";
        private const string SUBJECT = "hows it going bob";
        private const string BODY = "yo dawg, sup?";

        TempMail.Api api;
        string domain;
        string emailAddress;

        [OneTimeSetUp]
        public void Setup()
        {
            this.api = new TempMail.Api(ApiKeys.TempMailTesting);
            var domains = this.api.GetDomains();

            if (domains.Data != null)
            {
                var gen = new TempMail.Rng();
                domain = domains.Data[gen.Next(0, domains.Data.Count - 1)].Replace("@", string.Empty);
                var userName = gen.NextString();
                emailAddress = $"{userName}@{domain}";
            }
        }

        [Test()]
        public void GetDomains()
        {
            var response = this.api.GetDomains();

            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNotEmpty(response.Data);
        }

        [Test()]
        public void GetEmails()
        {
            var response = this.api.GetEmails(emailAddress);

            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            Assert.IsNotEmpty(response.Data);

            var client = new LookupClient { UseCache = true };
            var records = client.Query(domain, QueryType.MX);

            Assert.IsNotNull(records);
            Assert.IsNotNull(records.Answers);

            var mxRecords = records.Answers.OfType<DnsClient.Protocol.MxRecord>();
            Assert.IsNotEmpty(mxRecords);

            string mxHost = mxRecords.First().Exchange.Value;
            var smtpClient = new SmtpClient(mxHost.Substring(0, mxHost.Length - 1), 25)
            {
                DeliveryFormat = SmtpDeliveryFormat.International,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                EnableSsl = false
            };

            Email.DefaultSender = new SmtpSender(smtpClient) { UseSsl = false };

            var email = Email.From(SENDER).To(emailAddress).Subject(SUBJECT).Body(BODY);

            email.Send();

            Thread.Sleep(5500);

            response = this.api.GetEmails(emailAddress);

            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNotEmpty(response.Data);

            var message = response.Data[0];

            Assert.IsNotNull(message);
            Assert.IsFalse(string.IsNullOrWhiteSpace(message.MailId));
            Assert.AreEqual(SENDER, message.MailFrom);
            Assert.AreEqual(SUBJECT, message.MailSubject);
            Assert.AreEqual(BODY, message.MailText.Replace("\n", string.Empty));

            var emailResponse = this.api.GetEmail(message.MailId);

            Assert.IsNotNull(emailResponse);
            Assert.AreEqual(HttpStatusCode.OK, emailResponse.StatusCode);
            Assert.IsNotNull(emailResponse.Data);

            var reqMessage = emailResponse.Data;

            Assert.IsNotNull(reqMessage);
            Assert.IsFalse(string.IsNullOrWhiteSpace(reqMessage.MailId));
            Assert.AreEqual(SENDER, reqMessage.MailFrom);
            Assert.AreEqual(SUBJECT, reqMessage.MailSubject);
            Assert.AreEqual(BODY, reqMessage.MailText.Replace("\n", string.Empty));

            var sourceResponse = this.api.GetMessageSource(message.MailId);

            Assert.IsNotNull(sourceResponse);
            Assert.AreEqual(HttpStatusCode.OK, sourceResponse.StatusCode);
            Assert.IsFalse(string.IsNullOrWhiteSpace(sourceResponse.StringData));
            Assert.IsNull(sourceResponse.Data);
        }

        [Test()]
        public void GetEmailAttachment()
        {
            var response = this.api.GetEmails(emailAddress);

            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            Assert.IsNotEmpty(response.Data);

            var client = new LookupClient { UseCache = true };
            var records = client.Query(domain, QueryType.MX);

            Assert.IsNotNull(records);
            Assert.IsNotNull(records.Answers);

            var mxRecords = records.Answers.OfType<DnsClient.Protocol.MxRecord>();
            Assert.IsNotEmpty(mxRecords);

            string mxHost = mxRecords.First().Exchange.Value;
            var smtpClient = new SmtpClient(mxHost.Substring(0, mxHost.Length - 1), 25)
            {
                DeliveryFormat = SmtpDeliveryFormat.International,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                EnableSsl = false
            };

            Email.DefaultSender = new SmtpSender(smtpClient) { UseSsl = false };

            string path = this.GetType().Assembly.Location;
            string fileName = Path.GetFileName(path);
            var email = Email.From(SENDER).To(emailAddress).Subject(SUBJECT).Body(BODY)
                             .Attach(new Attachment
                             {
                                 Filename = fileName,
                                 ContentType = "application/octet-stream",
                                 Data = new FileStream(path, FileMode.Open)
                             });

            email.Send();

            Thread.Sleep(5500);

            response = this.api.GetEmails(emailAddress);

            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNotEmpty(response.Data);

            var message = response.Data[0];

            Assert.IsNotNull(message);
            Assert.IsFalse(string.IsNullOrWhiteSpace(message.MailId));
            Assert.AreEqual(SENDER, message.MailFrom);
            Assert.AreEqual(SUBJECT, message.MailSubject);
            Assert.AreEqual(BODY, message.MailText.Replace("\n", string.Empty));

            var attachmentResponse = this.api.GetMessageAttachment(message.MailId);

            Assert.IsNotNull(attachmentResponse);
            Assert.AreEqual(HttpStatusCode.OK, attachmentResponse.StatusCode);
            Assert.IsNotEmpty(attachmentResponse.Data);

            var attachments = attachmentResponse.Data;

            Assert.IsNotNull(attachments);
            Assert.IsNotEmpty(attachments);

            var attList = attachments[0];

            Assert.IsNotEmpty(attList);
            Assert.IsNotEmpty(attList);

            var att = attList[0];

            Assert.IsNotNull(att);
            Assert.AreEqual("base64", att.Header.ContentTransferEncoding.ToLower());
            Assert.IsTrue(att.Header.ContentType.StartsWith("application/octet-stream;", System.StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(att.Header.ContentType.EndsWith($"name={fileName}", System.StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(att.Header.ContentDisposition.StartsWith("attachment;", System.StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(att.Header.ContentDisposition.EndsWith($"filename={fileName}", System.StringComparison.CurrentCultureIgnoreCase));
            Assert.IsFalse(string.IsNullOrWhiteSpace(att.Body));

            var sourceResponse = this.api.GetMessageSource(message.MailId);

            Assert.IsNotNull(sourceResponse);
            Assert.AreEqual(HttpStatusCode.OK, sourceResponse.StatusCode);
            Assert.IsFalse(string.IsNullOrWhiteSpace(sourceResponse.StringData));
            Assert.IsNull(sourceResponse.Data);
        }
    }
}
