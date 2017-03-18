namespace BullOak.Common.WebApi.Extensions
{
    using Newtonsoft.Json;
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    public static class HttpClientExtensions
    {
        public static Task<HttpResponseMessage> PatchAsJsonAsync<T>(this HttpClient client, string requestUri, T value)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (string.IsNullOrEmpty(requestUri)) throw new ArgumentNullException(nameof(requestUri));
            if (value == null) throw new ArgumentNullException(nameof(value));

            var request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri)
            {
                Content = new StringContent(JsonConvert.SerializeObject(value), Encoding.UTF8, "application/json"),
            };

            return client.SendAsync(request);
        }
    }
}
