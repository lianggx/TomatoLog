using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TomatoLog.Common.Utilities
{
    public class HttpHelper
    {
        private static readonly HttpClient httpClient = null;

        static HttpHelper()
        {
            httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
            httpClient.DefaultRequestHeaders.Clear();
        }

        public static HttpClient Create(int expireSeconds)
        {
            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(expireSeconds);
            client.DefaultRequestHeaders.Clear();
            return client;
        }

        public async static Task<string> HttpRequest(HttpClient client, string url, HttpMethod method, string data = null, string contentType = "application/json")
        {
            client = client ?? httpClient;
            string result = string.Empty;
            DateTime dt = DateTime.Now;
            try
            {
                HttpResponseMessage response = null;
                if (method == HttpMethod.Post)
                {
                    HttpContent content = new StringContent(data ?? "", Encoding.UTF8, contentType);
                    response = await client.PostAsync(url, content);
                }
                else if (method == HttpMethod.Get)
                {
                    response = await client.GetAsync(url + data);
                }

                if (response != null && response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    result = await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception e)
            {
                var ex = e;
                if (e.InnerException != null)
                    if (e.InnerException.InnerException != null)
                        ex = e.InnerException.InnerException;
                    else
                        ex = e.InnerException;
                throw ex;
            }
            return result;
        }

        public static async Task<string> GetFrom(string url, int? timeout = null)
        {
            var return_str = "";
            DateTime dt = DateTime.Now;
            try
            {
                HttpClient client = timeout.HasValue ? Create(timeout.Value) : null;
                return_str = await HttpRequest(client, url, HttpMethod.Get, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return return_str;
        }
    }
}