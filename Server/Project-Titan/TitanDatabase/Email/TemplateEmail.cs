using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Utils;

namespace TitanDatabase.Email
{
    public class TemplateEmail
    {
        private TemplateString htmlMessage;

        private TemplateString message;

        public TemplateEmail(string htmlMessage, string message, char keyCharacter)
        {
            this.htmlMessage = new TemplateString(htmlMessage, keyCharacter);
            this.message = new TemplateString(message, keyCharacter);
        }

        public void Build(Dictionary<string, string> keyValues, out string htmlMessage, out string message)
        {
            htmlMessage = this.htmlMessage.Build(keyValues);
            message = this.message.Build(keyValues);
        }
    }
}
