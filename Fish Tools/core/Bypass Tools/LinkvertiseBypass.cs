using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Fish_Tools.core.Utils;
using static Fish_Tools.core.BypassTools.LinkvertiseBypassLib;

namespace Fish_Tools.core.BypassTools
{
    public class LinkvertiseBypass
    {
        private readonly HttpHandler _httpHandler = new();

        private readonly HttpHandler.RequestHeadersEx[] _headers =
        {
            new("Origin", "https://linkvertise.com"),
            new("Referer", "https://linkvertise.com/"),
            new("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv,109.0) Gecko/20100101 Firefox/119.0"),
            new("Accept", "application/json"),
            new("Accept-Language", "en-CA,en-US;q=0.7,en;q=0.3"),
            new("Sec-Fetch-Dest", "empty"),
            new("Sec-Fetch-Mode", "cors"),
            new("Sec-Fetch-Site", "same-site")
        };
        private (string id, string name) ParseUrl(Uri uri)
        {
            string id, name;
            string[] segments = uri.AbsolutePath.Trim('/').Split('/');
            if (segments.Length < 2) { throw new Exception("Invalid path segments"); }
            if (uri.AbsolutePath.Contains("download")) { id = segments[1]; name = segments[2]; }
            else { id = segments[0]; name = segments[1]; }

            return (id, name);
        }
        public async Task<string> ExecuteGraphQlRequestAsync<T>(T requestData, string endpoint, Func<JsonElement, string> resultSelector)
        {
            JsonDocument jsonData = JsonSerializer.SerializeToDocument(requestData);
            string requestJson = jsonData.RootElement.GetProperty("root").ToString();

            HttpResponseMessage responseData = await _httpHandler.PostAsync(endpoint, HttpMethod.Post, requestJson, _headers);
            string response = responseData.Content.ReadAsStringAsync().Result;

            return resultSelector(JsonDocument.Parse(response).RootElement.GetProperty("data"));
        }
        public T CreateRequestData<T>(string userId, string url, string? accessToken = null, string? adCompletedToken = null) where T : BaseRequest
        {
            T request = Activator.CreateInstance<T>();
            request.root = new BaseRequest.RootMain
            {
                OperationName = BaseRequest.GetOperation<T>(),
                Query = BaseRequest.GetMutationQuery<T>(),
                Variables = new BaseRequest.Variables
                {
                    LinkIdentification = new BaseRequest.LinkIdentification { UserIdAndUrl = new BaseRequest.UserIdAndUrl { UserId = userId, Url = url } },
                    Origin = "https://linkvertise.com/",
                    AdditionalData = new BaseRequest.AdditionalData { Taboola = new BaseRequest.Taboola { UserId = "", Url = "" } }
                }
            };

            if (typeof(T) == typeof(AdCompletedToken)) { request.root.Variables.CompleteDetailPageContent = new AdCompletedToken.CompleteDetailPageContent { access_token = accessToken }; }
            else if (typeof(T) == typeof(FinalResponse)) { request.root.Variables.Token = adCompletedToken; }
            return request;
        }
        public async Task Bypass(Uri uri, Logger Logger)
        {
            (string id, string name) = ParseUrl(uri);

            Logger.Warn("Loading...");

            AdAccessToken accessData = CreateRequestData<AdAccessToken>(id, name);
            string adToken = await ExecuteGraphQlRequestAsync(accessData, "https://publisher.linkvertise.com/graphql", data => data.GetProperty("getDetailPageContent").GetProperty("access_token").ToString());

            Logger.Warn("Loading...");

            AdCompletedToken adCompletedData = CreateRequestData<AdCompletedToken>(id, name, adToken);
            string adCompletedToken = await ExecuteGraphQlRequestAsync(adCompletedData, "https://publisher.linkvertise.com/graphql", data => data.GetProperty("completeDetailPageContent").GetProperty("TARGET").ToString());

            Logger.Warn("Loading...");

            FinalResponse finalResponse = CreateRequestData<FinalResponse>(id, name, null, adCompletedToken);
            string finalUrl = await ExecuteGraphQlRequestAsync(finalResponse, "https://publisher.linkvertise.com/graphql", data => data.GetProperty("getDetailPageTarget").GetProperty("url").ToString());

            Logger.Success(finalUrl);
            System.Diagnostics.Process.Start(new ProcessStartInfo { FileName = finalUrl, UseShellExecute = true });
            Console.ReadLine();
        }
    }

    public class LinkvertiseBypassLib
    {
        public static void Info()
        {
            string MadeBy = "Trollicus, RiisDev, and kaisei-kto";
            string Url = "https://github.com/Trollicus/silver-fishstick";
            Console.WriteLine("Made By" + MadeBy);
            Console.WriteLine("Url" + Url);
            Console.WriteLine("If " + MadeBy + "Wants Me to Take this down dm me on discord [notfishvr___]");
        }

        // HttpHandler
        public class HttpHandler
        {
            private readonly HttpClient? _client;

            /// <summary>
            /// Initializes a new instance of the HttpHandler class with optional proxy configuration.
            /// </summary>
            /// <param name="proxyHost">The proxy host, if a proxy should be used.</param>
            /// <param name="proxyPort">The proxy port, if a proxy should be used.</param>
            public HttpHandler(string proxyHost = "", int proxyPort = 0)
            {
                var clientHandler = new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.Brotli | DecompressionMethods.GZip | DecompressionMethods.Deflate,
                    SslProtocols = System.Security.Authentication.SslProtocols.Tls12,
                    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true,
                };

                if (!string.IsNullOrEmpty(proxyHost) && proxyPort > 0)
                {
                    clientHandler.Proxy = new WebProxy(proxyHost, proxyPort);
                    clientHandler.UseProxy = true;
                }

                _client = new HttpClient(clientHandler, true);
            }


            /// <summary>
            /// Asynchronously sends a POST request with JSON content and custom headers to the specified URI.
            /// </summary>
            /// <param name="uri">The URI to send the request to.</param>
            /// <param name="method">The HTTP method to use for the request.</param>
            /// <param name="json">The JSON content to include in the request body.</param>
            /// <param name="requestHeaders">An array of RequestHeadersEx objects representing custom headers for the request.</param>
            /// <returns>A task representing the asynchronous operation, with a result containing the HttpResponseMessage.</returns>
            public async Task<HttpResponseMessage> PostAsync(string uri, HttpMethod method, string json, RequestHeadersEx[] requestHeaders)
            {
                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(uri),
                    Method = method,
                    Content = new StringContent(json) { Headers = { ContentType = new MediaTypeHeaderValue("application/json") } },
                };
                foreach (var requestHeader in requestHeaders) { request.Headers.Add(requestHeader.Key, requestHeader.Value); }
                Debug.Assert(_client != null, nameof(_client) + " != null");
                return await _client.SendAsync(request);
            }

            public record RequestHeadersEx(string Key, string? Value);
        }

        // TypeHandler
        public class FinalResponse : BaseRequest { }
        public class AdAccessToken : BaseRequest { }
        public class AdCompletedToken : BaseRequest
        {
            public class CompleteDetailPageContent
            {
                [JsonPropertyName("access_token")]
                public string? access_token { get; set; }
            }
        }

        // BaseRequest
        public class BaseRequest
        {
            public RootMain root { get; set; }

            public class LinkIdentification
            {
                [JsonPropertyName("userIdAndUrl")]
                public UserIdAndUrl UserIdAndUrl { get; set; }
            }

            public class RootMain
            {
                [JsonPropertyName("operationName")]
                public string OperationName { get; set; }

                [JsonPropertyName("variables")]
                public Variables Variables { get; set; }

                [JsonPropertyName("query")]
                public string Query { get; set; }
            }

            public class UserIdAndUrl
            {
                [JsonPropertyName("user_id")]
                public string UserId { get; set; }

                [JsonPropertyName("url")]
                public string Url { get; set; }
            }

            public class Variables
            {
                [JsonPropertyName("linkIdentificationInput")]
                public LinkIdentification LinkIdentification { get; set; }

                [JsonPropertyName("origin")]
                public string Origin { get; set; }

                [JsonPropertyName("additional_data")]
                public AdditionalData AdditionalData { get; set; }

                [JsonPropertyName("completeDetailPageContentInput")]
                public AdCompletedToken.CompleteDetailPageContent? CompleteDetailPageContent { get; set; }

                [JsonPropertyName("token")]
                public string? Token { get; set; }
            }

            public class AdditionalData
            {
                [JsonPropertyName("taboola")]
                public Taboola Taboola { get; set; }
            }

            public class Taboola
            {
                [JsonPropertyName("user_id")]
                public string UserId { get; set; }

                [JsonPropertyName("url")]
                public string Url { get; set; }
            }

            public static string GetOperation<T>()
            {
                if (typeof(T) == typeof(AdAccessToken))
                {
                    return "getDetailPageContent";
                }

                if (typeof(T) == typeof(AdCompletedToken))
                {
                    return "completeDetailPageContent";
                }

                if (typeof(T) == typeof(FinalResponse))
                {
                    return "getDetailPageTarget";
                }

                return "";
            }

            public static string GetMutationQuery<T>()
            {
                if (typeof(T) == typeof(AdAccessToken))
                {
                    return "mutation getDetailPageContent($linkIdentificationInput: PublicLinkIdentificationInput!, $origin: String, $additional_data: CustomAdOfferProviderAdditionalData!) {  getDetailPageContent(    linkIdentificationInput: $linkIdentificationInput    origin: $origin    additional_data: $additional_data  ) {    access_token    premium_subscription_active    link {      video_url      short_link_title      recently_edited      short_link_title      description      url      seo_faqs {        body        title        __typename      }      target_host      last_edit_at      link_images {        url        __typename      }      title      thumbnail_url      view_count      is_trending      recently_edited      seo_faqs {        title        body        __typename      }      percentage_rating      is_premium_only_link      publisher {        id        name        subscriber_count        __typename      }      __typename    }    linkCustomAdOffers {      title      call_to_action      description      countdown      completion_token      provider      provider_additional_payload {        taboola {          available_event_url          visible_event_url          __typename        }        __typename      }      media {        type        ... on UrlMediaResource {          content_type          resource_url          __typename        }        __typename      }      clickout_action {        type        ... on CustomAdOfferClickoutUrlAction {          type          clickout_url          __typename        }        __typename      }      __typename    }    link_recommendations {      short_link_title      target_host      id      url      publisher {        id        name        __typename      }      last_edit_at      link_images {        url        __typename      }      title      thumbnail_url      view_count      is_trending      recently_edited      percentage_rating      publisher {        name        __typename      }      __typename    }    target_access_information {      remaining_accesses      daily_access_limit      __typename    }    __typename  }}";
                }

                if (typeof(T) == typeof(AdCompletedToken))
                {
                    return "mutation completeDetailPageContent($linkIdentificationInput: PublicLinkIdentificationInput!, $completeDetailPageContentInput: CompleteDetailPageContentInput!) {  completeDetailPageContent(    linkIdentificationInput: $linkIdentificationInput    completeDetailPageContentInput: $completeDetailPageContentInput  ) {    CUSTOM_AD_STEP    TARGET    __typename  }}";
                }

                if (typeof(T) == typeof(FinalResponse))
                {
                    return "mutation getDetailPageTarget($linkIdentificationInput: PublicLinkIdentificationInput!, $token: String!) {  getDetailPageTarget(    linkIdentificationInput: $linkIdentificationInput    token: $token  ) {    type    url    paste    short_link_title    __typename  }}";
                }

                return "";
            }
        }
    }
}