using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Utils.NET.Logging;

namespace TitanDatabase.Email
{
    public static class Emailer
    {
        private static AmazonSimpleEmailServiceClient client = new AmazonSimpleEmailServiceClient(AwsConstants.Key, AwsConstants.Secret, RegionEndpoint.USWest2);

        private const string Email_Source = "noreply@trialsoftitan.com";

        private const string Web_Server_Url =
#if DEBUG
            "https://local.trialsoftitan.com:8443";
#else
            "https://web.trialsoftitan.com";
#endif

        private static TemplateEmail verificationTemplate = new TemplateEmail(
"Welcome to Trials of Titan! <br><br>Before you can play, we need to verify your email! <br><br>Just visit this link to verify your email address: <br><br>#link <br><br>This verification link will expire in 24 hours",

"Welcome to Trials of Titan! \n\nBefore you can play, we need to verify your email! \n\nJust visit this link to verify your email address: \n\n#link \n\nThis verification link will expire in 24 hours", '#');

        public static Task<bool> SendVerificationEmail(string address, string token)
        {
            return Task.FromResult(true);
            /*
            verificationTemplate.Build(new Dictionary<string, string>
            {
                { "link", $"{Web_Server_Url}/v1/account/verify?token={token}" }
            }, out var htmlMessage, out var message);
            return await SendEmail(new SendEmailRequest()
            {
                Source = Email_Source,
                Destination = new Destination
                {
                    ToAddresses = new List<string> { address }
                },
                Message = new Message
                {
                    Subject = new Content("[Trials of Titan] Verify your email"),
                    Body = new Body
                    {
                        Html = new Content
                        {
                            Charset = "UTF-8",
                            Data = htmlMessage
                        },
                        Text = new Content
                        {
                            Charset = "UTF-8",
                            Data = message
                        } 
                    }
                }
            });
            */
        }

        public static void SendBugReport(string message)
        {
            /*
            var task = Task.Run(() => SendEmail(new SendEmailRequest()
            {
                Source = Email_Source,
                Destination = new Destination
                {
                    ToAddresses = new List<string> { "--- REPORTING EMAIL ---" }
                },
                Message = new Message
                {
                    Subject = new Content("[Trials of Titan] Bug Report"),
                    Body = new Body
                    {
                        Text = new Content
                        {
                            Charset = "UTF-8",
                            Data = message
                        }
                    }
                }
            }));
            task.GetAwaiter().GetResult();
            */
        }

        private static async Task<bool> SendEmail(SendEmailRequest request)
        {
            try
            {
                var response = await client.SendEmailAsync(request);
                return response.HttpStatusCode == HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                Log.Write(e);
                return false;
            }
        }

    }
}
