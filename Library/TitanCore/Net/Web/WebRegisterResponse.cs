using System;
using System.Collections.Generic;
using System.Text;

namespace TitanCore.Net.Web
{
    public enum WebRegisterResult
    {
        Success,
        DuplicateName,
        DuplicateEmail,
        InternalServerError,
        InvalidRequest,
        RateLimitExceeded
    }

    public class WebRegisterResponse
    {
        public WebRegisterResult result;

        public string accessToken;

        public WebRegisterResponse()
        {

        }

        public WebRegisterResponse(WebRegisterResult result, string accessToken)
        {
            this.result = result;
            this.accessToken = accessToken;
        }
    }
}
