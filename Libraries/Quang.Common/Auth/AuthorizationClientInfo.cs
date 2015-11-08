namespace Quang.Common.Auth
{
    public class AuthorizationClientInfo
    {
        public string UserName { get; set; }

        public string ClientCode { get; set; }

        public AuthorizationClientInfo()
          : this(null, null)
        {
        }

        public AuthorizationClientInfo(string userName, string clientCode)
        {
            UserName = userName;
            ClientCode = clientCode;
        }
    }
}
