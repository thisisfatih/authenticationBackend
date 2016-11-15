using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Config;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.Azure.Mobile.Server.Authentication;
using Newtonsoft.Json;

namespace authenticationBackend.Controllers
{
	[MobileAppController]
	[Authorize]
	public class gpController : ApiController
	{
		// GET api/gp
		public async System.Threading.Tasks.Task<string> Get()
		{
			try
			{
				var claimsPrincipal = this.User as ClaimsPrincipal;
				string sid = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier).Value;

				var credentials = await this.User.GetAppServiceIdentityAsync<GoogleCredentials>(this.Request);

				if (credentials.Provider == "Google")
				{
					// Create a query string with the Google access token.
					var requestUrl = "https://www.googleapis.com/oauth2/v3/userinfo?access_token=" + credentials.AccessToken;

					// Create an HttpClient request.
					var client = new System.Net.Http.HttpClient();

					// Request the current user info from Google.
					var resp = await client.GetAsync(requestUrl);
					resp.EnsureSuccessStatusCode();

					// Do something here with the Google user information.
					var info = await resp.Content.ReadAsStringAsync();

					googleData gd;

					if (info != null)
					{
						//object version
						gd = JsonConvert.DeserializeObject<googleData>(info.ToString());

						//return json
						return info;
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
	public class googleData
	{
		public string id { get; set; }
		public string sub { get; set; }
		public string name { get; set; }
		public string given_name { get; set; }
		public string family_name { get; set; }
		public string picture { get; set; }
		public string email { get; set; }
		public bool email_verified { get; set; }
		public string locale { get; set; }
	}
}
