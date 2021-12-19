using System;
using System.Collections.Generic;
using System.Text;

namespace TitanCore.Net.Web
{
    public enum WebNameChangeResult
    {
        Success,
        InvalidRequest,
        NameTaken,
        RateLimitExceeded,
        NotEnoughCurrency,
        InternalServerError
    }

    public class WebNameChangeResponse
    {
        public WebNameChangeResult result;

        public string newName;

        public WebNameChangeResponse()
        {

        }

        public WebNameChangeResponse(WebNameChangeResult result, string newName)
        {
            this.result = result;
            this.newName = newName;
        }
    }
}
