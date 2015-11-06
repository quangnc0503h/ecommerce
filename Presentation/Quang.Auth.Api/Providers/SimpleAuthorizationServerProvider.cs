using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using Quang.Auth.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;

namespace Quang.Auth.Api.Providers
{
    public class SimpleAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            string clientId = string.Empty;
            string clientSecret = string.Empty;
            Client client = (Client)null;
            if (!context.TryGetBasicCredentials(out clientId, out clientSecret))
                context.TryGetFormCredentials(out clientId, out clientSecret);
            if (context.ClientId == null)
            {
                context.Validated();
                return (Task)Task.FromResult<object>((object)null);
            }
            using (AuthRepository authRepository = new AuthRepository())
                client = authRepository.FindClient(context.ClientId);
            if (client == null)
            {
                context.SetError("invalid_clientId", string.Format("Client '{0}' is not registered in the system.", (object)context.ClientId));
                return (Task)Task.FromResult<object>((object)null);
            }
            if (client.ApplicationType == ApplicationTypes.NativeConfidential)
            {
                if (string.IsNullOrEmpty(clientSecret))
                {
                    context.SetError("invalid_clientId", "Client secret should be sent.");
                    return (Task)Task.FromResult<object>((object)null);
                }
                if (client.Secret != Helper.GetHash(clientSecret))
                {
                    context.SetError("invalid_clientId", "Client secret is invalid.");
                    return (Task)Task.FromResult<object>((object)null);
                }
            }
            if (!client.Active)
            {
                context.SetError("invalid_clientId", "Client is inactive.");
                return (Task)Task.FromResult<object>((object)null);
            }
            context.OwinContext.Set<string>("as:clientAllowedOrigin", client.AllowedOrigin);
            context.OwinContext.Set<string>("as:clientRefreshTokenLifeTime", client.RefreshTokenLifeTime.ToString());
            context.Validated();
            return (Task)Task.FromResult<object>((object)null);
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            string allowedOrigin = context.OwinContext.Get<string>("as:clientAllowedOrigin") ?? "*";
            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new string[1]
            {
        allowedOrigin
            });
            using (AuthRepository authRepository = new AuthRepository())
            {
                IdentityUser user = await authRepository.FindUser(context.UserName, context.Password);
                if (user == null)
                {
                    context.SetError("invalid_grant", "The user name or password is incorrect.");
                    goto label_8;
                }
            }
            ClaimsIdentity identity = new ClaimsIdentity(context.Options.AuthenticationType);
            identity.AddClaim(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", context.UserName));
            identity.AddClaim(new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "user"));
            identity.AddClaim(new Claim("sub", context.UserName));
            AuthenticationProperties props = new AuthenticationProperties((IDictionary<string, string>)new Dictionary<string, string>()
      {
        {
          "as:client_id",
          context.ClientId == null ? string.Empty : context.ClientId
        },
        {
          "userName",
          context.UserName
        }
      });
            AuthenticationTicket ticket = new AuthenticationTicket(identity, props);
            context.Validated(ticket);
            label_8:;
        }

        public override Task GrantRefreshToken(OAuthGrantRefreshTokenContext context)
        {
            if (context.Ticket.Properties.Dictionary["as:client_id"] != context.ClientId)
            {
                context.SetError("invalid_clientId", "Refresh token is issued to a different clientId.");
                return (Task)Task.FromResult<object>((object)null);
            }
            ClaimsIdentity identity = new ClaimsIdentity((IIdentity)context.Ticket.Identity);
            Claim claim = Enumerable.FirstOrDefault<Claim>(Enumerable.Where<Claim>(identity.Claims, (Func<Claim, bool>)(c => c.Type == "newClaim")));
            if (claim != null)
                identity.RemoveClaim(claim);
            identity.AddClaim(new Claim("newClaim", "newValue"));
            AuthenticationTicket ticket = new AuthenticationTicket(identity, context.Ticket.Properties);
            context.Validated(ticket);
            return (Task)Task.FromResult<object>((object)null);
        }

        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (KeyValuePair<string, string> keyValuePair in (IEnumerable<KeyValuePair<string, string>>)context.Properties.Dictionary)
                context.AdditionalResponseParameters.Add(keyValuePair.Key, (object)keyValuePair.Value);
            return (Task)Task.FromResult<object>((object)null);
        }
    }
}