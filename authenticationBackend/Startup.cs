using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(authenticationBackend.Startup))]

namespace authenticationBackend
{
	public partial class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			ConfigureMobileApp(app);
		}
	}
}