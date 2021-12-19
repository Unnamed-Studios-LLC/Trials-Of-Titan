using System;
using System.Collections.Generic;
using System.Text;

namespace TitanCore.Net.Web
{
    public enum WebLoginResult
    {
        Success,
        InvalidRequest,
        RateLimitExceeded,
        InternalServerError,
        InvalidEmail,
        InvalidHash
    }

    public class WebLoginResponse
    {
        public WebLoginResult result;

        public string accessToken;

        public WebLoginResponse()
        {

        }

        public WebLoginResponse(WebLoginResult result, string accessToken)
        {
            this.result = result;
            this.accessToken = accessToken;
        }
    }
}
