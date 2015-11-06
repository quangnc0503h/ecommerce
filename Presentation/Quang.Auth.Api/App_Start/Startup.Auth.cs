using System;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using Owin;
using Microsoft.Owin.Security.Facebook;
using Microsoft.Owin.Security.Google;
using System.Configuration;
using System.Threading.Tasks;
using Quang.Auth.Api.Providers;
using Quang.Auth.Api.Models;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.AspNet.Identity.Owin;

namespace Quang.Auth.Api
{
    public partial class Startup
    {
        public static readonly TimeSpan AccessTokenExpireTimeSpan = TimeSpan.FromDays(1.0);

        public static OAuthAuthorizationServerOptions OAuthOptions { get; private set; }

        public static FacebookAuthenticationOptions facebookAuthOptions { get; private set; }

        public static GoogleOAuth2AuthenticationOptions googleAuthOptions { get; private set; }

        public static string PublicClientId { get; private set; }

        public void ConfigureAuth(IAppBuilder app)
        {
            AppBuilderExtensions.CreatePerOwinContext<ApplicationDbContext>(app, new Func<ApplicationDbContext>(ApplicationDbContext.Create));
            AppBuilderExtensions.CreatePerOwinContext<ApplicationUserManager>(app, new Func<IdentityFactoryOptions<ApplicationUserManager>, IOwinContext, ApplicationUserManager>(ApplicationUserManager.Create));
            AppBuilderExtensions.CreatePerOwinContext<ApplicationRoleManager>(app, new Func<IdentityFactoryOptions<ApplicationRoleManager>, IOwinContext, ApplicationRoleManager>(ApplicationRoleManager.Create));
            CookieAuthenticationExtensions.UseCookieAuthentication(app, new CookieAuthenticationOptions());
            AppBuilderExtensions.UseExternalSignInCookie(app, "ExternalCookie");
            Startup.PublicClientId = "ngAuthApp";
            Startup.OAuthOptions = new OAuthAuthorizationServerOptions()
            {
                TokenEndpointPath = new PathString("/token"),
                Provider = (IOAuthAuthorizationServerProvider)new ApplicationOAuthProvider(Startup.PublicClientId),
                RefreshTokenProvider = (IAuthenticationTokenProvider)new SimpleRefreshTokenProvider(),
                AuthorizeEndpointPath = new PathString("/api/Account/ExternalLogin"),
                AccessTokenExpireTimeSpan = Startup.AccessTokenExpireTimeSpan,
                AllowInsecureHttp = true
            };
            AppBuilderExtensions.UseOAuthBearerTokens(app, Startup.OAuthOptions);
            Startup.facebookAuthOptions = new FacebookAuthenticationOptions()
            {
                AppId = ConfigurationManager.AppSettings["OAuth.Facebook.AppId"],
                AppSecret = ConfigurationManager.AppSettings["OAuth.Facebook.AppSecret"],
                Provider = (IFacebookAuthenticationProvider)new FacebookAuthProvider()
            };
            Startup.facebookAuthOptions.Scope.Add("email");
            FacebookAuthenticationExtensions.UseFacebookAuthentication(app, Startup.facebookAuthOptions);
            Startup.googleAuthOptions = new GoogleOAuth2AuthenticationOptions()
            {
                ClientId = ConfigurationManager.AppSettings["OAuth.Google.ClientId"],
                ClientSecret = ConfigurationManager.AppSettings["OAuth.Google.ClientSecret"],
                Provider = (IGoogleOAuth2AuthenticationProvider)new GoogleAuthProvider()
            };
            Startup.googleAuthOptions.Scope.Add("email");
            GoogleAuthenticationExtensions.UseGoogleAuthentication(app, Startup.googleAuthOptions);
        }


    }
}
