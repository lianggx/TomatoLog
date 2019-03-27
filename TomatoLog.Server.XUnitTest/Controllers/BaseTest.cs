using TomatoLog.Server;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace DinnerNowHostXUnitTest
{
    public abstract class BaseTest
    {
        protected static TestServer server;
        protected static HttpClient httpClient;
        protected static ITestOutputHelper output = null;

        public BaseTest(ITestOutputHelper outPut)
        {
            if (server == null)
            {
                output = outPut;
                server = new TestServer(new WebHostBuilder().UseStartup<Startup>());
                httpClient = server.CreateClient();
                // httpClient.DefaultRequestHeaders.Add("", "");
            }
        }

        public async Task<object> PostData(string action, dynamic body)
        {
            var content = new StringContent(JsonConvert.SerializeObject(body));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            content.Headers.Add("X-Real-IP", "127.0.0.1");
            var data = await httpClient.PostAsync(action, content);
            var result = await data.Content.ReadAsStringAsync();
            output.WriteLine(result);
            var apiReturn = JsonConvert.DeserializeObject<object>(result);

            return apiReturn;
        }

        public async Task<object> GetData(string action)
        {
            var data = await httpClient.GetAsync(action);
            var result = await data.Content.ReadAsStringAsync();
            output.WriteLine(result);
            var apiReturn = JsonConvert.DeserializeObject<object>(result);

            return apiReturn;
        }
    }
}
