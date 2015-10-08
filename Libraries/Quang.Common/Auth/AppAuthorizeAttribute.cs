using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace Quang.Common.Auth
{
    public class AppAuthorizeAttribute : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            base.HandleUnauthorizedRequest(actionContext);
            if (!string.IsNullOrEmpty(Roles))
            {
                var responseContent = actionContext.Response.Content as System.Net.Http.ObjectContent<HttpError>;
                if (responseContent != null)
                {
                    var contentError = responseContent.Value as HttpError;
                    if (contentError != null && string.IsNullOrEmpty(contentError.MessageDetail))
                    {
                        var customContentError = new HttpError(contentError.Message);
                       // var roleList = ActionRole.ToListDictionary();
                        if (actionContext.ControllerContext.RequestContext.Principal.Identity.IsAuthenticated)
                        {
                            var roleList = ActionRole.ToListDictionary();
                            string role = Roles;
                            foreach (var item in roleList)
                            {
                                if (item.Key == Roles)
                                {
                                    role = item.Value.RoleKeyLabel;
                                }
                            }

                            customContentError.MessageDetail = string.Format("You must have permission in [{0}] to access this page.\n\nPls contact admin to support.", role);
                            actionContext.Response.Content = new ObjectContent<HttpError>(customContentError, responseContent.Formatter);
                        }
                        else
                        {
                            customContentError.MessageDetail = "You need login to access this page.";
                            actionContext.Response.Content = new ObjectContent<HttpError>(customContentError, responseContent.Formatter);
                        }
                    }
                }
            }
        }
    }
}
