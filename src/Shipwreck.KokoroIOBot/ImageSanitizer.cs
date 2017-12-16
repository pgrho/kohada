using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Shipwreck.KokoroIOBot
{
    internal class ImageSanitizer
    {
        public static string GyazoAccessToken { get; set; }

        public static async Task<string> GetSafeUrlAsync(string baseUrl)
        {
            using (var hc = new HttpClient())
            {
                var oa = await hc.GetAsync(baseUrl).ConfigureAwait(false);

                using (var data = await oa.Content.ReadAsStreamAsync().ConfigureAwait(false))
                {
                    var req = new HttpRequestMessage(HttpMethod.Post, "https://upload.gyazo.com/api/upload");

                    var mp = new MultipartFormDataContent();

                    mp.Add(new StringContent(GyazoAccessToken), "access_token");

                    var sc = new StreamContent(data);

                    sc.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                    {
                        Name = "imagedata",
                        FileName = "temp.jpg"
                    };
                    mp.Add(sc, "imagedata");

                    req.Content = mp;

                    var res = await hc.SendAsync(req).ConfigureAwait(false);

                    res.EnsureSuccessStatusCode();

                    var json = await res.Content.ReadAsStringAsync().ConfigureAwait(false);

                    var jo = JObject.Parse(json);

                    return jo.Property("url")?.Value?.Value<string>();
                }
            }
        }
    }
}