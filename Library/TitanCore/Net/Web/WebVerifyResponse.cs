using System;
using System.Collections.Generic;
using System.Text;

namespace TitanCore.Net.Web
{
    public class WebVerifyResponse
    {
        public bool success;

        public WebVerifyResponse()
        {

        }

        public WebVerifyResponse(bool success)
        {
            this.success = success;
        }
    }
}
