
using Microsoft.Owin;
using Newtonsoft.Json.Linq;
using Quang.RedisCache;
using Serilog;
using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;

namespace Quang.Common.Auth
{
    public class ClientApiProvider
    {
        private const string ClientTokenCacheKey = "GET_CLIENT_OAUTH_ACCESS_TOKEN_{0}_{1}";
        private const UInt64 ClientRequestMaxAgeInSeconds = 5; // Max time of client token should live in 5 seconds

        public const string OAuthHeaderName = "Authorization";
        public const string OAuthHeaderScheme = "Bearer";
        public const string ClientHeaderAuthName = "Authorization";
        public const string ClientHeaderAuthScheme = "cliamx";
        public const string ClientTokenUserName = "dsvn-c1db31a7-27df-4e64-9cac-c1b238ea2b80";
        public const string ClientTokenPassword = "0IdM+3dxTteHMp1VT2Rl5Vb2Z3fK8Pkbm8O7VgV9SBE=";
        
        private static readonly ICacheClient _redisCache = new RedisCacheClient(RedisConnection.SecurityConn);

        private const string ClientTokenCacheTimeInMinutesAppSettingsKey = "AppClientTokenCacheTimeInMinutes";
        public const UInt64 ClientTokenCacheTimeInMinutesDefault = 10;
        public static UInt64 ClientTokenCacheTimeInMinutes
        {
            get
            {
                UInt64 value = ClientTokenCacheTimeInMinutesDefault;
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings[ClientTokenCacheTimeInMinutesAppSettingsKey]))
                {
                    UInt64.TryParse(ConfigurationManager.AppSettings[ClientTokenCacheTimeInMinutesAppSettingsKey], out value);
                }
                return value;
            }
        }

        public static async Task<string> GetAccessToken(AuthenticationHeaderValue authenticationHeaderValue, IOwinRequest request)
        {
            if (authenticationHeaderValue.Scheme == ClientApiProvider.ClientHeaderAuthScheme && !string.IsNullOrEmpty(authenticationHeaderValue.Parameter))
            {
                var autherizationHeaderArray = GetAutherizationHeaderValues(authenticationHeaderValue.Parameter);
                var apiKey = autherizationHeaderArray[0];
                var cacheKeyClientToken = string.Format(ClientTokenCacheKey, apiKey, "token");
                var cacheKeyClientSecret = string.Format(ClientTokenCacheKey, apiKey, "secret");
                var accessClientToken = _redisCache.Get<string>(cacheKeyClientToken);
                var accessClientSecret = _redisCache.Get<string>(cacheKeyClientSecret);
                if (!string.IsNullOrEmpty(accessClientToken) && !string.IsNullOrEmpty(accessClientSecret))
                {
                    string incomingBase64Signature = autherizationHeaderArray[1];
                    string nonce = autherizationHeaderArray[2];
                    string requestTimeStamp = autherizationHeaderArray[3];
                    string requestUri = HttpUtility.UrlEncode(request.Uri.AbsoluteUri.ToLower());
                    string requestMethod = HttpUtility.UrlEncode(request.Method);

                    var isValid = await ClientApiProvider.isValidRequest(requestUri, requestMethod, apiKey, accessClientSecret, incomingBase64Signature, nonce, requestTimeStamp);
                    if (isValid)
                    {
                        return accessClientToken;
                    }
                }
                else
                {
                    var accessTokenResult = await GetAccessTokenFromAuthServer(authenticationHeaderValue, request).ConfigureAwait(false) ;
                    if (accessTokenResult != null)
                    {
                        if (ClientTokenCacheTimeInMinutes > 0)
                        {
                            await _redisCache.AddAsync(cacheKeyClientToken, accessTokenResult.Item1, TimeSpan.FromMinutes(ClientTokenCacheTimeInMinutes));
                            await _redisCache.AddAsync(cacheKeyClientSecret, accessTokenResult.Item2, TimeSpan.FromMinutes(ClientTokenCacheTimeInMinutes));
                        }
                        return accessTokenResult.Item1;
                    }

                }
            }
            return null;
        }

        private static async Task<Tuple<string, string>> GetAccessTokenFromAuthServer(AuthenticationHeaderValue authenticationHeaderValue, IOwinRequest request)
        {
            string oauthTokenUrl = WebConfigurationManager.AppSettings["OAuth.TokenUrl"];
            Tuple<string, string> accessTokenResult = null;

            using (HttpClient client = HttpClientFactory.Create())
            {
                string username = HttpUtility.UrlEncode(ClientTokenUserName);
                string password = HttpUtility.UrlEncode(ClientTokenPassword);
                string authenticationValue = HttpUtility.UrlEncode(authenticationHeaderValue.ToString());
                string requestUri = HttpUtility.UrlEncode(request.Uri.AbsoluteUri.ToLower());
                string requestUriEncoded = HttpUtility.UrlEncode(Convert.ToBase64String(Encoding.UTF8.GetBytes(requestUri)));
                string requestMethod = HttpUtility.UrlEncode(request.Method);
                string clientHostName = HttpContext.Current.Request.UserHostName;
                string clientHostAddress = HttpContext.Current.Request.UserHostAddress;

                var dataUrl = string.Format("grant_type=password&username={0}&password={1}&cauthorization={2}&curi={3}&cmethod={4}&chostname={5}&chostaddress={6}", username, password, authenticationValue, requestUriEncoded, requestMethod, clientHostName, clientHostAddress);
                HttpResponseMessage response = await client.PostAsync(oauthTokenUrl, new StringContent(dataUrl, Encoding.UTF8)).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var responseObj = JObject.Parse(responseString);
                    if (responseObj["access_token"] != null && responseObj["client_api_secret"] != null)
                    {
                        string accessToken = responseObj["access_token"].ToString();
                        string client_api_secret = responseObj["client_api_secret"].ToString();
                        accessTokenResult = new Tuple<string, string>(accessToken, client_api_secret);
                    }

                }
            }
            return accessTokenResult;
        }

        public static async Task<bool> isValidRequest(string clientRequestUri, string clientRequestMethod, string apiKey, string apiSecret, string incomingBase64Signature, string nonce, string requestTimeStamp)
        {
            bool isValid = false;
            bool isReplayRequest = checkReplayRequest(nonce, requestTimeStamp);
            if (isReplayRequest)
            {
                return false;
            }

            string data = String.Format("{0}{1}{2}{3}{4}", apiKey, clientRequestMethod.ToLower(), clientRequestUri, requestTimeStamp, nonce);

            var apiSecretBytes = Convert.FromBase64String(apiSecret);

            byte[] signature = Encoding.UTF8.GetBytes(data);

            using (HMACSHA256 hmac = new HMACSHA256(apiSecretBytes))
            {
                byte[] signatureBytes = hmac.ComputeHash(signature);
                string signatureString = Convert.ToBase64String(signatureBytes);
                isValid = (incomingBase64Signature.Equals(signatureString, StringComparison.Ordinal));
                if (!isValid)
                {
                    Log.Logger.Information(string.Format("Invalid client request: [{0}][{1}][{2}]", apiKey, clientRequestMethod, clientRequestUri));
                    Log.Logger.Information("incoming-signature:" + incomingBase64Signature);
                    Log.Logger.Information("current--signature:" + signatureString);
                }
            }
            return await Task.FromResult(isValid);
        }

        public static string[] GetAutherizationHeaderValues(string rawAuthzHeader)
        {
            string clientAuthorization = Encoding.UTF8.GetString(Convert.FromBase64String(rawAuthzHeader));
            var credArray = clientAuthorization.Split(':');

            if (credArray.Length == 4)
            {
                return credArray;
            }
            else
            {
                return null;
            }

        }

        private static bool checkReplayRequest(string nonce, string requestTimeStamp)
        {
            string cacheKey = string.Format("{0}_{1}", MethodInfo.GetCurrentMethod().ToString(), nonce);
            string oldRequestTimeStamp = _redisCache.Get<string>(cacheKey);
            if (!string.IsNullOrEmpty(oldRequestTimeStamp))
            {
                return true;
            }

            DateTime epochStart = new DateTime(1970, 01, 01, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan currentTs = DateTime.UtcNow - epochStart;

            var serverTotalSeconds = Convert.ToUInt64(currentTs.TotalSeconds);
            var requestTotalSeconds = Convert.ToUInt64(requestTimeStamp);

            if ((serverTotalSeconds - requestTotalSeconds) > ClientRequestMaxAgeInSeconds)
            {
                return true;
            }

            _redisCache.Add(nonce, requestTimeStamp, DateTimeOffset.UtcNow.AddSeconds(ClientRequestMaxAgeInSeconds));

            return false;
        }

        public static string GenerateApiKey()
        {
            return Guid.NewGuid().ToString("N");
        }

        public static string GenerateApiSecret()
        {
            using (var cryptoProvider = new RNGCryptoServiceProvider())
            {
                byte[] secretKeyByteArray = new byte[64];

                cryptoProvider.GetBytes(secretKeyByteArray);

                return Convert.ToBase64String(secretKeyByteArray);
            }
        }
    }
}
