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

	public class fbController : ApiController
    {
		// GET api/fb
		public async System.Threading.Tasks.Task<string> Get()
		{
			try
			{
				var claimsPrincipal = this.User as ClaimsPrincipal;
				string sid = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier).Value;

				var credentials = await this.User.GetAppServiceIdentityAsync<FacebookCredentials>(this.Request);

				if (credentials.Provider == "Facebook")
				{
					// Create a query string with the Facebook access token.
					var requestUrl = "https://graph.facebook.com/v2.8/me?fields=email%2Cname%2Clast_name%2Cfriends%2Cfirst_name%2Cis_verified%2Cverified%2Cgender%2Cage_range%2Cid%2Cname_format%2Cupdated_time%2Clink%2Cinstalled%2Cinstall_type%2Ctimezone%2Cis_shared_login%2Clocale&access_token=" + credentials.AccessToken;

					// Create an HttpClient request.
					var client = new System.Net.Http.HttpClient();

					// Request the current user info from Facebook.
					var resp = await client.GetAsync(requestUrl);
					resp.EnsureSuccessStatusCode();

					// Do something here with the Facebook user information.
					var info = await resp.Content.ReadAsStringAsync();

					facebookData fd = new facebookData();

					if (info != null)
					{
						//object version
						fd = JsonConvert.DeserializeObject<facebookData>(info.ToString());
						
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
	public class facebookData
	{
		public string email { get; set; }
		public string name { get; set; }
		public string last_name { get; set; }
		public Friends friends { get; set; }
		public string first_name { get; set; }
		public bool is_verified { get; set; }
		public bool verified { get; set; }
		public string gender { get; set; }
		public Age_Range age_range { get; set; }
		public string id { get; set; }
		public string name_format { get; set; }
		public string updated_time { get; set; }
		public string link { get; set; }
		public bool installed { get; set; }
		public string install_type { get; set; }
		public int timezone { get; set; }
		public bool is_shared_login { get; set; }
		public string locale { get; set; }
		public string facebookCredsID { get; set; }
		public string fbid { get; set; }
	}
	public class Friends
	{
		public facebookDataFriends[] data { get; set; }
		public Paging paging { get; set; }
		public facebookDataFriendsSummary summary { get; set; }
	}
	public class Paging
	{
		public Cursors cursors { get; set; }
	}
	public class Cursors
	{
		public string before { get; set; }
		public string after { get; set; }
	}
	public class facebookDataFriendsSummary
	{
		public string id { get; set; }
		public string facebookCredsID { get; set; }
		public int total_count { get; set; }
	}
	public class facebookDataFriends
	{
		public string id { get; set; }
		public string facebookCredsID { get; set; }
		public string name { get; set; }
		public string fbid { get; set; }
	}
	public class Age_Range
	{
		public int min { get; set; }
	}
}
