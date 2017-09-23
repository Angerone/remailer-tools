using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using RestSharp;

namespace TempMail
{
    public class Api
    {
        private const string URL = "https://privatix-temp-mail-v1.p.mashape.com/request/";
        private readonly string apiKey;

        public Api(string apiKey)
        {
            this.apiKey = apiKey;
        }

        public ApiResponse<List<string>> GetDomains()
        {
            var (client, request) = PrepareRequest("domains/");
            var response = client.Execute<List<string>>(request);
            return new ApiResponse<List<string>>(response);
        }

        public ApiResponse<Email> GetEmail(string messageId)
        {
            var (client, request) = PrepareRequest($"one_mail/id/{messageId}/");
            var response = client.Execute<Email>(request);
            return new ApiResponse<Email>(response);
        }

        public ApiResponse<List<Email>> GetEmails(string address)
        {
            string hash = string.Empty;

            using (var md5 = new MD5CryptoServiceProvider())
            {
                var bytes = md5.ComputeHash(Encoding.ASCII.GetBytes(address));
                hash = string.Join(string.Empty, bytes.Select(b => b.ToString("x2")));
            }

            var (client, request) = PrepareRequest($"mail/id/{hash}/");
            var response = client.Execute<List<Email>>(request);
            return new ApiResponse<List<Email>>(response);
        }

        public ApiResponse<List<List<Message>>> GetMessageAttachment(string messageId)
        {
            var (client, request) = PrepareRequest($"attachments/id/{messageId}/");
            var response = client.Execute<List<List<Message>>>(request);
            return new ApiResponse<List<List<Message>>>(response);
        }

        public ApiResponse<Message> GetMessage(string messageId)
        {
            var (client, request) = PrepareRequest($"one_mail/id/{messageId}/");
            var response = client.Execute<Message>(request);
            return new ApiResponse<Message>(response);
        }

        public ApiResponse<string> GetMessageSource(string messageId)
        {
            var (client, request) = PrepareRequest($"source/id/{messageId}/");
            var response = client.Execute(request);
            return new ApiResponse<string>(response);
        }

        private (RestClient Client, RestRequest Request) PrepareRequest(string verb)
        {
            var client = new RestClient(URL);
            var request = new RestRequest(verb);
            request.AddHeader("X-Mashape-Key", this.apiKey);
            request.AddHeader("Accept", "application/json");

            return (client, request);
        }
    }
}
