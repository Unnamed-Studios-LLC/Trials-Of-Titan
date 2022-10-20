using System;
using System.Collections.Generic;

namespace Utils.NET.Utils
{
    public class TemplateEmail
    {
        public char keyCharacter;

        public TemplateString htmlMessage;

        public TemplateString message;

        public TemplateEmail(string htmlMessage, string message, char keyCharacter)
        {
            this.keyCharacter = keyCharacter;
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
