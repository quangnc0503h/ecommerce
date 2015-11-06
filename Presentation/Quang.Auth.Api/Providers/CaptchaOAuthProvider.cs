using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using Quang.Auth.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace Quang.Auth.Api.Providers
{
    public class CaptchaOAuthProvider : OAuthAuthorizationServerProvider
    {
        private readonly string _publicClientId;

        public CaptchaOAuthProvider(string publicClientId)
        {
            if (publicClientId == null)
                throw new ArgumentNullException("publicClientId");
            this._publicClientId = publicClientId;
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            IFormCollection formCollection = await context.Request.ReadFormAsync();
            CaptchaData captcha = new CaptchaData()
            {
                CaptchaChallenge = context.UserName,
                CaptchaResponse = context.Password,
                UserHostAddress = context.Request.LocalIpAddress,
                ClientId = context.ClientId
            };
            CaptchaOutput captchaOutput = await this.ValidateCaptcha(captcha);
            if (captchaOutput == null || !captchaOutput.Status)
            {
                context.SetError("invalid_captcha", "Mã bảo vệ chưa đúng, bạn vui lòng nhập lại!");
            }
            else
            {
                ApplicationUserManager userManager = OwinContextExtensions.GetUserManager<ApplicationUserManager>(context.OwinContext);
                ApplicationUser user = await userManager.FindAsync("e7c44459-837c-45f2-b125-2b639d84ea45", "abcd@1234A");
                ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync((UserManager<ApplicationUser>)userManager, "Bearer");
                ClaimsIdentity cookiesIdentity = await user.GenerateUserIdentityAsync((UserManager<ApplicationUser>)userManager, "Cookies");
                AuthenticationProperties properties = new AuthenticationProperties();
                properties.Dictionary.Add(new KeyValuePair<string, string>("client_id", captchaOutput.ClientId));
                AuthenticationTicket ticket = new AuthenticationTicket(oAuthIdentity, properties);
                context.Validated(ticket);
                context.Request.Context.Authentication.SignIn(cookiesIdentity);
            }
        }

        public async Task<CaptchaOutput> ValidateCaptcha(CaptchaData captchaData)
        {
            CaptchaOutput result = new CaptchaOutput()
            {
                Status = false,
                Msg = ""
            };
            RecaptchaVerificationHelper verify = new RecaptchaVerificationHelper()
            {
                Challenge = captchaData.CaptchaChallenge,
                Response = captchaData.CaptchaResponse,
                PrivateKey = "6LeX2cgSAAAAAKkTWQSP6lO7xYsq_v4UF1BM_iCi",
                UserHostAddress = captchaData.UserHostAddress,
                UseSsl = false
            };
            RecaptchaVerificationResult response = await verify.VerifyRecaptchaResponseTaskAsync();
            if (response == RecaptchaVerificationResult.Success)
            {
                result.Status = true;
            }
            else
            {
                result.Status = false;
                result.Msg = "Mã bảo vệ chưa đúng, bạn vui lòng nhập lại!";
            }
            result.ClientId = string.IsNullOrEmpty(captchaData.ClientId) ? Guid.NewGuid().ToString("n") : captchaData.ClientId;
            result.AccessToken = string.Empty;
            await Task.Delay(1);
            return result;
        }

        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (KeyValuePair<string, string> keyValuePair in (IEnumerable<KeyValuePair<string, string>>)context.Properties.Dictionary)
                context.AdditionalResponseParameters.Add(keyValuePair.Key, (object)keyValuePair.Value);
            return (Task)Task.FromResult<object>((object)null);
        }

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            if (context.ClientId == null)
                context.Validated();
            return (Task)Task.FromResult<object>((object)null);
        }

        public override Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
        {
            if (context.ClientId == this._publicClientId && new Uri(context.Request.Uri, "/").AbsoluteUri == context.RedirectUri)
                context.Validated();
            return (Task)Task.FromResult<object>((object)null);
        }

        public static AuthenticationProperties CreateProperties(string userName, OAuthGrantResourceOwnerCredentialsContext context)
        {
            return new AuthenticationProperties((IDictionary<string, string>)new Dictionary<string, string>()
      {
        {
          "as:client_id",
          context.ClientId ?? string.Empty
        },
        {
          "userName",
          userName
        }
      });
        }
    }
}