
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
        private static readonly ICacheClient _redisCache = new RedisCacheClient(RedisConnection.SecurityConn);
        public const string OAuthHeaderName = "Authorization";
        public const string OAuthHeaderScheme = "Bearer";
        public const string ClientHeaderAuthName = "Authorization";
        public const string ClientHeaderAuthScheme = "Cliamx";
        public const string ClientTokenUserName = "dsvn-c1db31a7-27df-4e64-9cac-c1b238ea2b80";
        public const string ClientTokenPassword = "0IdM+3dxTteHMp1VT2Rl5Vb2Z3fK8Pkbm8O7VgV9SBE=";
        public const ulong ClientTokenCacheTimeInMinutesDefault = 10UL;

        public static ulong ClientTokenCacheTimeInMinutes
        {
            get
            {
                ulong result = 10UL;
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["AppClientTokenCacheTimeInMinutes"]))
                    ulong.TryParse(ConfigurationManager.AppSettings["AppClientTokenCacheTimeInMinutes"], out result);
                return result;
            }
        }

        public static async Task<string> GetAccessToken(AuthenticationHeaderValue authenticationHeaderValue, IOwinRequest request)
        {
            string str = string.Empty;
            if (authenticationHeaderValue.Scheme == "Cliamx" && !string.IsNullOrEmpty(authenticationHeaderValue.Parameter))
            {
                string[] autherizationHeaderArray = GetAutherizationHeaderValues(authenticationHeaderValue.Parameter);
                string apiKey = autherizationHeaderArray[0];
                var cacheKeyClientToken = string.Format("GET_CLIENT_OAUTH_ACCESS_TOKEN_{0}_{1}", apiKey, "token");
                var cacheKeyClientSecret = string.Format("GET_CLIENT_OAUTH_ACCESS_TOKEN_{0}_{1}", apiKey, "secret");
                var accessClientToken = _redisCache.Get<string>(cacheKeyClientToken);
                var accessClientSecret = _redisCache.Get<string>(cacheKeyClientSecret);
                if (!string.IsNullOrEmpty(accessClientToken) && !string.IsNullOrEmpty(accessClientSecret))
                {
                   // Log.Logger.Information("get access token from redis cache: " + accessClientSecret + accessClientToken);
                    if (isValidRequest(request.Uri.AbsoluteUri.ToLower(), request.Method.ToLower(), apiKey, accessClientSecret, autherizationHeaderArray[1], autherizationHeaderArray[2], autherizationHeaderArray[3]))
                    {
                       // Log.Logger.Information("Valid token");
                        str = accessClientToken;
                        
                    }
                    else
                        Log.Logger.Information("Invalid token");
                }
                else
                {
                   // Log.Logger.Information("get access token from auth server");
                    Tuple<string, string> accessTokenResult = await GetAccessTokenFromAuthServer(authenticationHeaderValue, request).ConfigureAwait(false);
                    if (accessTokenResult != null)
                    {
                       // Log.Logger.Information("get access token from auth server success");
                        if (ClientTokenCacheTimeInMinutes > 0UL)
                        {
                            int num1 = await _redisCache.AddAsync(cacheKeyClientToken, accessTokenResult.Item1, TimeSpan.FromMinutes(ClientTokenCacheTimeInMinutes)) ? 1 : 0;
                            int num2 = await _redisCache.AddAsync(cacheKeyClientSecret, accessTokenResult.Item2, TimeSpan.FromMinutes(ClientTokenCacheTimeInMinutes)) ? 1 : 0;
                        }
                        str = accessTokenResult.Item1;
            
                    }
                    else
                        Log.Logger.Information("get access token from auth server failure");
                }
            }
            
            
            return str;
        }

        private static async Task<Tuple<string, string>> GetAccessTokenFromAuthServer(AuthenticationHeaderValue authenticationHeaderValue, IOwinRequest request)
        {
            string oauthTokenUrl = WebConfigurationManager.AppSettings["OAuth.TokenUrl"];
            var accessTokenResult = (Tuple<string, string>)null;
            try
            {
                using (HttpClient httpClient = HttpClientFactory.Create())
                {
                    string username = HttpUtility.UrlEncode("dsvn-c1db31a7-27df-4e64-9cac-c1b238ea2b80");
                    string password = HttpUtility.UrlEncode("0IdM+3dxTteHMp1VT2Rl5Vb2Z3fK8Pkbm8O7VgV9SBE=");
                    string authenticationValue = HttpUtility.UrlEncode(authenticationHeaderValue.ToString());
                    string requestUriEncoded = HttpUtility.UrlEncode(Convert.ToBase64String(Encoding.UTF8.GetBytes(request.Uri.AbsoluteUri.ToLower())));
                    string clientHostName = SecurityUtils.GetClientHostName();
                    string clientHostAddress = SecurityUtils.GetClientIPAddress();
                    string dataUrl = string.Format("grant_type=password&username={0}&password={1}&cauthorization={2}&curi={3}&cmethod={4}&chostname={5}&chostaddress={6}", (object)username, (object)password, (object)authenticationValue, (object)requestUriEncoded, (object)request.Method.ToLower(), (object)clientHostName, (object)clientHostAddress);
                    Log.Logger.Information("get access token from auth server - data url: " + dataUrl);
                    HttpResponseMessage response = await httpClient.PostAsync(oauthTokenUrl, (HttpContent)new StringContent(dataUrl, Encoding.UTF8)).ConfigureAwait(false);
                    if (response.IsSuccessStatusCode)
                    {
                        //Log.Logger.Debug("Duong1: Reuqest token success");
                        string responseString = await response.Content.ReadAsStringAsync();
                        JObject responseObj = JObject.Parse(responseString);
                        if (responseObj["access_token"] != null)
                        {
                            if (responseObj["client_api_secret"] != null)
                            {
                                if (responseObj["userClientId"] != null)
                                    accessTokenResult = new Tuple<string, string>(responseObj["access_token"].ToString(), responseObj["client_api_secret"].ToString());
                            }
                        }
                    }
                    else
                        Log.Logger.Debug("Duong1: Reuqest token err");
                }
            }
            catch (Exception ex)
            {
               // Log.Logger.Error(ex.ToString());
                accessTokenResult = null;
            }
            return accessTokenResult;
        }

        public static bool isValidRequest(string clientRequestUri, string clientRequestMethod, string apiKey, string apiSecret, string incomingSignatureHash, string nonce, string requestTimeStamp)
        {
            bool flag = false;
            Log.Logger.Debug("isValidRequest-start-checkReplayRequest[nonce={0},requestTimeStamp={1}]", new object[2]{nonce,requestTimeStamp});
            if (checkReplayRequest(nonce, requestTimeStamp))
            {
                Log.Logger.Debug("isValidRequest: replay request");
                return false;
            }
            string str1 = string.Format("{0}{1}{2}{3}{4}", (object)apiKey, (object)clientRequestMethod.ToLower(), (object)clientRequestUri.ToLower(), (object)nonce, (object)requestTimeStamp);
           // Log.Logger.Debug("isValidRequest-info-clientRequestMethod:" + clientRequestMethod.ToLower());
           // Log.Logger.Debug("isValidRequest-info-clientRequestUri:" + clientRequestUri.ToLower());
         //  Log.Logger.Debug("isValidRequest-info-apiKey:" + apiKey);
         //   Log.Logger.Debug("isValidRequest-info-nonce:" + nonce);
         //   Log.Logger.Debug("isValidRequest-info-timeStamp:" + requestTimeStamp);
        //    Log.Logger.Debug("isValidRequest-info-value:" + str1);
            string str2 = SecurityUtils.MD5Hash(apiSecret + str1).ToLower();
            if (str2.ToLower() == incomingSignatureHash.ToLower())
            {
                flag = true;
                //Log.Logger.Debug("isValidRequest: okay");
            }
            else
            {
                Log.Logger.Debug(string.Format("Invalid client request: [{0}][{1}][{2}]", (object)apiKey, (object)HttpUtility.UrlDecode(clientRequestMethod), (object)HttpUtility.UrlDecode(clientRequestUri)));
                Log.Logger.Debug("isValidRequest-incoming-signature-hash:" + incomingSignatureHash);
                Log.Logger.Debug("isValidRequest-current--signature-hash:" + str2);
            }
            return flag;
        }

        public static string[] GetAutherizationHeaderValues(string rawAuthzHeader)
        {
            string[] strArray = Encoding.UTF8.GetString(Convert.FromBase64String(rawAuthzHeader)).Split(':');
            if (strArray.Length == 4)
                return strArray;
            return null;
        }

        private static bool checkReplayRequest(string nonce, string requestTimeStamp)
        {
            string key = string.Format("Client_access_nonce_{0}", nonce);
            if (!string.IsNullOrEmpty(ClientApiProvider._redisCache.Get<string>(key)))
            {
                Log.Logger.Debug("checkReplayRequest-nonce: replay - " + nonce);
                return true;
            }
            long num1 = Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);
            long num2 = Convert.ToInt64(requestTimeStamp);
            //Log.Logger.Debug("checkReplayRequest-nonce: " + nonce);
           // Log.Logger.Debug("checkReplayRequest-requestTimeStamp: " + requestTimeStamp);
            //Log.Logger.Debug("checkReplayRequest-requestTotalSeconds: " + num2);
            //Log.Logger.Debug("checkReplayRequest-serverTotalSeconds: " + num1);
           // Log.Logger.Debug("checkReplayRequest-value: " + (num1 - num2));
            if (Math.Abs(num1 - num2) > 60L)
                return true;
            _redisCache.Add(key, requestTimeStamp, DateTimeOffset.UtcNow.AddSeconds(60.0));
            return false;
        }

        public static string GenerateApiKey()
        {
            return Guid.NewGuid().ToString("N");
        }

        public static string GenerateApiSecret()
        {
            using (var cryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                var numArray = new byte[64];
                cryptoServiceProvider.GetBytes(numArray);
                return Convert.ToBase64String(numArray);
            }
        }
    }
}
