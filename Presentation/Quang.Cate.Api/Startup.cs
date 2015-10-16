using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Quang.Cate.Api.Startup))]

namespace Quang.Cate.Api
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll); //need on top, very importance            
            ConfigureAuth(app);
        }
    }
}
