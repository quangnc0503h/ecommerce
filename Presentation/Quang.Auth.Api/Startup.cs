using Quang.Auth.Api;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Startup))]

namespace Quang.Auth.Api
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll); //need on top, very importance
            ConfigureAuth(app);
            LogConfig.SetupLog();
           
        }
    }
}
