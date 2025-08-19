using System.Net.Http.Headers;
using System.Xml.Serialization;

namespace Kleer
{
    public class KleerClient : IDisposable
    {
        private readonly HttpClient _httpClient;

        public KleerClient(string token, string baseUrl = "https://api.kleer.se/v1/")
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("API token is required", nameof(token));

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/")
            };

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            _httpClient.DefaultRequestHeaders.Add("X-Token", token);
        }

        /// <summary>
        /// Build a preconfigured request (user can add content, headers etc.)
        /// </summary>
        public static HttpRequestMessage BuildRequest(HttpMethod method, string endpoint)
        {
            var request = new HttpRequestMessage(method, endpoint);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            return request;
        }

        /// <summary>
        /// Build a preconfigured request with serialized XML content
        /// </summary>
        public static HttpRequestMessage BuildXmlRequest<T>(HttpMethod method, string endpoint, T data)
        {
            var request = BuildRequest(method, endpoint);
            request.Content = new StringContent(KleerXmlSerializer.Serialize(data));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
            return request;
        }
        
        /// <summary>
        /// Build a request with binary content (e.g. for uploading receipts or attachments).
        /// </summary>
        public static HttpRequestMessage BuildBinaryRequest(HttpMethod method, string endpoint, byte[] data)
        {
            var request = new HttpRequestMessage(method, endpoint)
            {
                Content = new ByteArrayContent(data)
            };

            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            return request;
        }

        /// <summary>
        /// Build a request with binary content from a stream (e.g. FileStream or MemoryStream).
        /// </summary>
        public static HttpRequestMessage BuildBinaryRequest(HttpMethod method, string endpoint, Stream stream)
        {
            var request = new HttpRequestMessage(method, endpoint)
            {
                Content = new StreamContent(stream)
            };

            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            return request;
        }


        /// <summary>
        /// Send a request and return raw HTTP response.
        /// </summary>
        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            return _httpClient.SendAsync(request);
        }

        /// <summary>
        /// Send a request and deserialize the XML response into T.
        /// </summary>
        public async Task<T> SendAsync<T>(HttpRequestMessage request)
        {
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Status: {response.StatusCode}, Body: {error}");
            }

            var xml = await response.Content.ReadAsStringAsync();

            return KleerXmlSerializer.Deserialize<T>(xml);
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }

        // Optional helper for XML serialization
        public static string SerializeToXml<T>(T obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj), "Cannot serialize null object to XML.");

            return KleerXmlSerializer.Serialize(obj);
        }
    }
}
