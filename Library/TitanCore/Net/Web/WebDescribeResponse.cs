using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;

namespace TitanCore.Net.Web
{
    public enum WebDescribeResult
    {
        Success,
        UnverifiedEmail,
        InvalidRequest,
        InvalidToken,
        InternalServerError,
        RateLimitReached,
        NameRequired,
        UpdateRequired
    }

    public class WebDescribeResponse
    {
        public WebDescribeResult result;

        public string requiredBuild;

        public ulong accountId;

        public long currency;

        public string name;

        public string email;

        public int maxCharacters;

        public ClassQuest[] classQuests;

        public WebCharacterInfo[] characters;

        public uint[] unlockedItems;

        public WebServerInfo[] servers;

        public WebDescribeResponse()
        {

        }

        public WebDescribeResponse(WebDescribeResult result)
        {
            this.result = result;
        }

        public WebDescribeResponse(WebDescribeResult result, string requiredBuild)
        {
            this.result = result;
            this.requiredBuild = requiredBuild;
        }

        public WebDescribeResponse(WebDescribeResult result, ulong accountId, long currency, string name, string email, int maxCharacters, ClassQuest[] classQuests, WebCharacterInfo[] characters, uint[] unlockedItems)
        {
            this.result = result;
            this.accountId = accountId;
            this.currency = currency;
            this.name = name;
            this.email = email;
            this.maxCharacters = maxCharacters;
            this.classQuests = classQuests;
            this.characters = characters;
            this.unlockedItems = unlockedItems;
        }
    }

    public class WebCharacterInfo
    {
        public ulong id;

        public ushort type;

        public ushort skin;

        public Item[] equips;
    }

    /*
    public struct UnlockedItem
    {
        public EmoteType type;

        public UnlockedEmote(EmoteType type)
        {
            this.type = type;
        }
    }
    */
}
