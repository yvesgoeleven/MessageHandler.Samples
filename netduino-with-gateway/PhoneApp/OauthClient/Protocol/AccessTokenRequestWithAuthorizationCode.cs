using System;

namespace WindowsAzure.Acs.Oauth2.Client.WinRT.Protocol
{
    public class AccessTokenRequestWithAuthorizationCode
        : AccessTokenRequest
    {
        public string Code
        {
            get
            {
                return base.Parameters["code"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("value");
                }
                base.Parameters["code"] = value;
            }
        }

        public Uri RedirectUri
        {
            get
            {
                if (base.Parameters["redirect_uri"] != null)
                {
                    return new Uri(base.Parameters["redirect_uri"]);
                }
                return null;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (!value.IsAbsoluteUri)
                {
                    throw new ArgumentException("");
                }
                base.Parameters["redirect_uri"] = value.AbsoluteUri;
            }
        }

        public AccessTokenRequestWithAuthorizationCode(Uri baseUri)
            : base(baseUri)
        {
        }

        public override void Validate()
        {
            if (string.IsNullOrEmpty(this.Code) || this.RedirectUri == null)
            {
                throw new OAuthMessageException("");
            }
            base.Validate();
        }
    }
}