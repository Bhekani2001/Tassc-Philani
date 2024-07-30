using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Tassc.Startup))]
namespace Tassc
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
