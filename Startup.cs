using Microsoft.Owin;
using Owin;
using System.Web;

[assembly: OwinStartup("Config",typeof(sentTeamMessage.Startup))]

namespace sentTeamMessage
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
