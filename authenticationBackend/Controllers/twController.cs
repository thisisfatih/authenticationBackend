using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Config;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.Azure.Mobile.Server.Authentication;
using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json;
namespace authenticationBackend.Controllers
{
    [MobileAppController]
	[Authorize]
	public class twController : ApiController
    {
		// GET api/tw
		public async System.Threading.Tasks.Task<string> Get()
		{
			try
			{
				var claimsPrincipal = this.User as ClaimsPrincipal;
				string sid = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier).Value;

				var credentials = await this.User.GetAppServiceIdentityAsync<TwitterCredentials>(this.Request);

				if (credentials.Provider == "Twitter")
				{
					var consumerKey = "app consumer Key";
					var consumerSecret = "app consumer Secret";

					var oauth_version = "1.0";
					var oauth_signature_method = "HMAC-SHA1";

					// unique request details
					var oauth_nonce = Convert.ToBase64String(
						new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));
					var timeSpan = DateTime.UtcNow
						- new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
					var oauth_timestamp = Convert.ToInt64(timeSpan.TotalSeconds).ToString();
					var resource_url = "https://api.twitter.com/1.1/account/verify_credentials.json";
					var requestQuery = "include_email=true";

					//var resource_url = "https://api.twitter.com/1.1/statuses/user_timeline.json";
					// create oauth signature
					var baseFormat = "oauth_consumer_key={0}&oauth_nonce={1}&oauth_signature_method={2}" +
									"&oauth_timestamp={3}&oauth_token={4}&oauth_version={5}";

					var baseString = string.Format(baseFormat,
												consumerKey,
												oauth_nonce,
												oauth_signature_method,
												oauth_timestamp,
												credentials.AccessToken,
												oauth_version
												);

					baseString = string.Concat("GET&", Uri.EscapeDataString(resource_url), "&" + Uri.EscapeDataString(requestQuery), "%26"
						, Uri.EscapeDataString(baseString));

					var compositeKey = string.Concat(Uri.EscapeDataString(consumerSecret),
											"&", Uri.EscapeDataString(credentials.AccessTokenSecret));

					string oauth_signature;
					using (HMACSHA1 hasher = new HMACSHA1(ASCIIEncoding.ASCII.GetBytes(compositeKey)))
					{
						oauth_signature = Convert.ToBase64String(
							hasher.ComputeHash(ASCIIEncoding.ASCII.GetBytes(baseString)));
					}

					// create the request header
					var headerFormat = "OAuth oauth_nonce=\"{0}\", oauth_signature_method=\"{1}\", " +
									   "oauth_timestamp=\"{2}\", oauth_consumer_key=\"{3}\", " +
									   "oauth_token=\"{4}\", oauth_signature=\"{5}\", " +
									   "oauth_version=\"{6}\"";

					var authHeader = string.Format(headerFormat,
											Uri.EscapeDataString(oauth_nonce),
											Uri.EscapeDataString(oauth_signature_method),
											Uri.EscapeDataString(oauth_timestamp),
											Uri.EscapeDataString(consumerKey),
											Uri.EscapeDataString(credentials.AccessToken),
											Uri.EscapeDataString(oauth_signature),
											Uri.EscapeDataString(oauth_version)
									);
					// make the request

					ServicePointManager.Expect100Continue = false;

					//var postBody = "screen_name=" + Uri.EscapeDataString(credentials.UserId);
					resource_url += "?include_email=true";
					//resource_url += "?" + postBody;
					HttpWebRequest request = (HttpWebRequest)WebRequest.Create(resource_url);
					request.Headers.Add("Authorization", authHeader);
					request.Method = "GET";
					request.ContentType = "application/x-www-form-urlencoded";


					WebResponse response = request.GetResponse();
					string responseData = new StreamReader(response.GetResponseStream()).ReadToEnd();

					twitterData td;

					if (responseData != null)
					{
						td = JsonConvert.DeserializeObject<twitterData>(responseData.ToString());
						//return json string
						return responseData;
					}
					else
					{
						return null;
					}
				}
				else
				{
					return null;
				}
			}
			catch (System.Exception ex)
			{
				return ex.ToString();
			}
		}
	}
	public class OAuth1
	{
		const string OAUTH_VERSION = "1.0";
		const string SIGNATURE_METHOD = "HMAC-SHA1";
		const long UNIX_EPOC_TICKS = 621355968000000000L;

		public string GetAuthorizationHeaderString(string method, string url, IDictionary<string, string> parameters, string consumerSecret, string accessTokenSecret)
		{
			string encodedAndSortedString = BuildEncodedSortedString(parameters);
			string signatureBaseString = BuildSignatureBaseString(method, url, encodedAndSortedString);
			string signingKey = BuildSigningKey(consumerSecret, accessTokenSecret);
			string signature = CalculateSignature(signingKey, signatureBaseString);
			string authorizationHeader = BuildAuthorizationHeaderString(encodedAndSortedString, signature);

			return authorizationHeader;
		}

		internal void AddMissingOAuthParameters(IDictionary<string, string> parameters)
		{
			if (!parameters.ContainsKey("oauth_timestamp"))
				parameters.Add("oauth_timestamp", GetTimestamp());

			if (!parameters.ContainsKey("oauth_nonce"))
				parameters.Add("oauth_nonce", GenerateNonce());

			if (!parameters.ContainsKey("oauth_version"))
				parameters.Add("oauth_version", OAUTH_VERSION);

			if (!parameters.ContainsKey("oauth_signature_method"))
				parameters.Add("oauth_signature_method", SIGNATURE_METHOD);
		}

		internal string BuildEncodedSortedString(IDictionary<string, string> parameters)
		{
			AddMissingOAuthParameters(parameters);

			return
				string.Join("&",
					(from parm in parameters
					 orderby parm.Key
					 select parm.Key + "=" + PercentEncode(parameters[parm.Key]))
					.ToArray());
		}

		internal virtual string BuildSignatureBaseString(string method, string url, string encodedStringParameters)
		{
			int paramsIndex = url.IndexOf('?');

			string urlWithoutParams = paramsIndex >= 0 ? url.Substring(0, paramsIndex) : url;

			return string.Join("&", new string[]
			{
			method.ToUpper(),
			PercentEncode(urlWithoutParams),
			PercentEncode(encodedStringParameters)
			});
		}

		internal virtual string BuildSigningKey(string consumerSecret, string accessTokenSecret)
		{
			return string.Format(
				CultureInfo.InvariantCulture, "{0}&{1}",
				PercentEncode(consumerSecret),
				PercentEncode(accessTokenSecret));
		}

		internal virtual string CalculateSignature(string signingKey, string signatureBaseString)
		{
			byte[] key = Encoding.UTF8.GetBytes(signingKey);
			byte[] msg = Encoding.UTF8.GetBytes(signatureBaseString);

			KeyedHashAlgorithm hasher = new HMACSHA1();
			hasher.Key = key;
			byte[] hash = hasher.ComputeHash(msg);

			return Convert.ToBase64String(hash);
		}

		internal virtual string BuildAuthorizationHeaderString(string encodedAndSortedString, string signature)
		{
			string[] allParms = (encodedAndSortedString + "&oauth_signature=" + PercentEncode(signature)).Split('&');
			string allParmsString =
				string.Join(", ",
					(from parm in allParms
					 let keyVal = parm.Split('=')
					 where parm.StartsWith("oauth") || parm.StartsWith("x_auth")
					 orderby keyVal[0]
					 select keyVal[0] + "=\"" + keyVal[1] + "\"")
					.ToList());
			return "OAuth " + allParmsString;
		}

		internal virtual string GetTimestamp()
		{
			long ticksSinceUnixEpoc = DateTime.UtcNow.Ticks - UNIX_EPOC_TICKS;
			double secondsSinceUnixEpoc = new TimeSpan(ticksSinceUnixEpoc).TotalSeconds;
			return Math.Floor(secondsSinceUnixEpoc).ToString(CultureInfo.InvariantCulture);
		}

		internal virtual string GenerateNonce()
		{
			return new Random().Next(111111, 9999999).ToString(CultureInfo.InvariantCulture);
		}

		internal virtual string PercentEncode(string value)
		{
			const string ReservedChars = @"`!@#$^&*()+=,:;'?/|\[] ";

			var result = new StringBuilder();

			if (string.IsNullOrWhiteSpace(value))
				return string.Empty;

			var escapedValue = Uri.EscapeDataString(value);

			// Windows Phone doesn't escape all the ReservedChars properly, so we have to do it manually.
			foreach (char symbol in escapedValue)
			{
				if (ReservedChars.IndexOf(symbol) != -1)
				{
					result.Append('%' + String.Format("{0:X2}", (int)symbol).ToUpper());
				}
				else
				{
					result.Append(symbol);
				}
			}

			return result.ToString();
		}
	}
	public class twitterData
	{
		public string id { get; set; }
		public string twitterCredsID { get; set; }
		public string id_str { get; set; }
		public string name { get; set; }
		public string screen_name { get; set; }
		public string location { get; set; }
		public string description { get; set; }
		public string url { get; set; }
		public bool @protected { get; set; }
		public int followers_count { get; set; }
		public int friends_count { get; set; }
		public int listed_count { get; set; }
		public string created_at { get; set; }
		public int favourites_count { get; set; }
		public int utc_offset { get; set; }
		public string time_zone { get; set; }
		public bool geo_enabled { get; set; }
		public bool verified { get; set; }
		public int statuses_count { get; set; }
		public string lang { get; set; }
		public bool contributors_enabled { get; set; }
		public bool is_translator { get; set; }
		public bool is_translation_enabled { get; set; }
		public string profile_background_color { get; set; }
		public string profile_background_image_url { get; set; }
		public string profile_background_image_url_https { get; set; }
		public bool profile_background_tile { get; set; }
		public string profile_image_url { get; set; }
		public string profile_image_url_https { get; set; }
		public string profile_banner_url { get; set; }
		public string profile_link_color { get; set; }
		public string profile_sidebar_border_color { get; set; }
		public string profile_sidebar_fill_color { get; set; }
		public string profile_text_color { get; set; }
		public bool profile_use_background_image { get; set; }
		public bool has_extended_profile { get; set; }
		public bool default_profile { get; set; }
		public bool default_profile_image { get; set; }
		public bool following { get; set; }
		public bool follow_request_sent { get; set; }
		public bool notifications { get; set; }
		public string translator_type { get; set; }
		public string email { get; set; }
	}
}
