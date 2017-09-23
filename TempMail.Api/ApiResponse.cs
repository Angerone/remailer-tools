using System.Net;
using RestSharp;

namespace TempMail
{
    public class ApiResponse<T>
    {
        public HttpStatusCode StatusCode { get; set; } = 0;

        public string StringData { get; set; }

        public T Data { get; set; }

        public ApiResponse(IRestResponse<T> response)
        {
            this.StatusCode = response.StatusCode;
            this.Data = response.Data;
            this.StringData = null;
        }

        public ApiResponse(IRestResponse response)
        {
            this.StatusCode = response.StatusCode;
            this.StringData = response.Content;
            this.Data = default(T);
        }
    }
}
