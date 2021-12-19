using System;
using System.Collections.Generic;
using System.Text;

namespace TitanCore.Net.Web
{
    public class WebSteamInitTxnResponse
    {
        public bool success;

        public ulong orderId;

        public WebSteamInitTxnResponse()
        {

        }

        public WebSteamInitTxnResponse(bool success)
        {
            this.success = success;
        }

        public WebSteamInitTxnResponse(bool success, ulong orderId)
        {
            this.success = success;
            this.orderId = orderId;
        }
    }
}
