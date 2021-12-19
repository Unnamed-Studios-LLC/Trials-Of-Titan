using System;
using System.Collections.Generic;
using System.Text;

namespace TitanCore.Net.Web
{
    public enum WebServerListResult
    {
        Success,
        InternalServerError,
        InvalidRequest,
        RateLimitExceeded
    }

    public enum ServerStatus
    {
        Normal,
        Crowded,
        Full
    }

    public class WebServerListResponse
    {
        public WebServerListResult result;

        public WebServerInfo[] servers;

        public WebServerListResponse()
        {

        }

        public WebServerListResponse(WebServerListResult result)
        {
            this.result = result;
        }

        public WebServerListResponse(WebServerListResult result, WebServerInfo[] servers)
        {
            this.result = result;
            this.servers = servers;
        }
    }

    public class WebServerInfo
    {
        public string name;

        public string host;

        public string pingHost;

        public ServerStatus status;

        public WebServerInfo()
        {

        }

        public WebServerInfo(string name, string host, string pingHost, ServerStatus status)
        {
            this.name = name;
            this.host = host;
            this.pingHost = pingHost;
            this.status = status;
        }
    }
}
