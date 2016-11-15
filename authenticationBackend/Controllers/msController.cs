using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Config;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.Azure.Mobile.Server.Authentication;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace authenticationBackend.Controllers
{
	[MobileAppController]
	[Authorize]

	public class msController : ApiController
	{
		// GET api/ms
		public async System.Threading.Tasks.Task<string> Get()
		{
			try
			{
				var claimsPrincipal = this.User as ClaimsPrincipal;
				string sid = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier).Value;

				var credentials = await this.User.GetAppServiceIdentityAsync<MicrosoftAccountCredentials>(this.Request);

				if (credentials.Provider == "MicrosoftAccount")
				{
					// Create a query string with the Microsoft access token.
					var requestUrl = "https://apis.live.net/v5.0/me/?method=GET&access_token=" + credentials.AccessToken;

					// Create an HttpClient request.
					var client = new System.Net.Http.HttpClient();

					// Request the current user info from Microsoft.
					var resp = await client.GetAsync(requestUrl);
					resp.EnsureSuccessStatusCode();

					// Do something here with the Microsoft user information.
					var info = await resp.Content.ReadAsStringAsync();

					microsoftData gd;

					if (info != null)
					{
						//object version
						gd = JsonConvert.DeserializeObject<microsoftData>(info.ToString());

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
	public class microsoftData
	{
		public string id { get; set; }
		public string name { get; set; }
		public string first_name { get; set; }
		public string last_name { get; set; }
		public string link { get; set; }
		public int birth_day { get; set; }
		public int birth_month { get; set; }
		public int birth_year { get; set; }
		public object gender { get; set; }
		public microsoftEmails emails { get; set; }
		public string locale { get; set; }
		public string updated_time { get; set; }
	}
	public class microsoftEmails
	{
		public string microsoftCredsId { get; set; }
		public string id { get; set; }

		public string preferred { get; set; }
		public string account { get; set; }
		public string personal { get; set; }
		public string business { get; set; }
	}
}
