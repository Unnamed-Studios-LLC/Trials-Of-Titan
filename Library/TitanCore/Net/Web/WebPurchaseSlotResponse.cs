using System;
using System.Collections.Generic;
using System.Text;

namespace TitanCore.Net.Web
{
    public enum WebPurchaseSlotResult
    {
        Success,
        NotEnoughGold,
        AccountInUse,
        InternalServerError,
        InvalidRequest,
        RateLimitReached
    }

    public class WebPurchaseSlotResponse
    {
        public WebPurchaseSlotResult result;

        public long currency;

        public WebPurchaseSlotResponse() { }

        public WebPurchaseSlotResponse(WebPurchaseSlotResult result, long currency)
        {
            this.result = result;
            this.currency = currency;
        }
    }
}
