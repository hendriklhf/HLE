using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace HLE.HttpRequests
{
    /// <summary>
    /// A class that performs a Http POST request on creation of the object.
    /// </summary>
    public class HttpPost
    {
        /// <summary>
        /// The URL of the request.
        /// </summary>
        public string Url { get; }

        /// <summary>
        /// The complete answer as a string.
        /// </summary>
        public string? Result { get; private set; }

        /// <summary>
        /// The header content that will be sent to the URL.
        /// </summary>
        public HttpContent HeaderContent { get; }

        /// <summary>
        /// The answer stored in a <see cref="JsonElement"/>, if the answer was a json compatible string.
        /// </summary>
        public JsonElement Data { get; }

        /// <summary>
        /// True, if the answer was a json compatible string, otherwise false.<br />
        /// If true, the JSON result has been stored in the property <see cref="Data"/>.
        /// </summary>
        public bool IsValidJsonData { get; } = true;

        private readonly HttpClient _httpClient = new();

        /// <summary>
        /// The main constructor of <see cref="HttpPost"/>.<br />
        /// The request will be executed in the constructor.
        /// </summary>
        /// <param name="url">The URL to which the request will be send to.</param>
        /// <param name="headers">The header content that will be sent to the URL.</param>
        public HttpPost(string url, IEnumerable<(string Key, string Value)> headers)
        {
            Url = url;
            IEnumerable<KeyValuePair<string, string>> headerCollection = headers.Select(h => new KeyValuePair<string, string>(h.Key, h.Value));
            HeaderContent = new FormUrlEncodedContent(headerCollection);
            Task.Run(async () => Result = await PostRequest()).Wait();
            try
            {
                if (string.IsNullOrEmpty(Result))
                {
                    throw new JsonException("HttpPost.Result is null or empty");
                }

                Data = JsonSerializer.Deserialize<JsonElement>(Result);
            }
            catch (JsonException)
            {
                IsValidJsonData = false;
            }
        }

        private async Task<string> PostRequest()
        {
            HttpResponseMessage response = await _httpClient.PostAsync(Url, HeaderContent);
            return await response.Content.ReadAsStringAsync();
        }
    }
}
