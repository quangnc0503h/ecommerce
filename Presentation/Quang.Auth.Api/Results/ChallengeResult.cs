using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Quang.Auth.Api.Results
{
    public class ChallengeResult : IHttpActionResult
    {
        public string LoginProvider { get; set; }

        public HttpRequestMessage Request { get; set; }

        public ChallengeResult(string loginProvider, ApiController controller)
        {
            this.LoginProvider = loginProvider;
            this.Request = controller.Request;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            OwinHttpRequestMessageExtensions.GetOwinContext(this.Request).Authentication.Challenge(this.LoginProvider);
            return Task.FromResult<HttpResponseMessage>(new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                RequestMessage = this.Request
            });
        }
    }
}
