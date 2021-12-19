using System.Collections.Generic;
using System.Linq;

namespace TitanCore.Iap
{
    public class IapProduct
    {
        public static IapProduct spareCurrency = new IapProduct("Spare Tokens", "com.UnnamedStudios.TrialsOfTitan.TokensSpare", 730240674007482429, 1, "$0.99", 99, 100);

        public static IapProduct sackOfCurrency = new IapProduct("Sack of Tokens", "com.UnnamedStudios.TrialsOfTitan.TokensSack", 730241540492099636, 2, "$4.99", 499, 550);

        public static IapProduct boxOfCurrency = new IapProduct("Box of Tokens", "com.UnnamedStudios.TrialsOfTitan.TokensBox", 730241833900310620, 3, "$9.99", 999, 1200);

        public static IapProduct chestOfCurrency = new IapProduct("Chest of Tokens", "com.UnnamedStudios.TrialsOfTitan.TokensChest", 730241950812471408, 4, "$19.99", 1999, 2600);

        public static IapProduct vaultOfCurrency = new IapProduct("Vault of Tokens", "com.UnnamedStudios.TrialsOfTitan.TokensVault", 730242099391365173, 5, "$49.99", 4999, 7000);

        public static IapProduct[] products = new IapProduct[]
        {
            spareCurrency,
            sackOfCurrency,
            boxOfCurrency,
            chestOfCurrency,
            vaultOfCurrency,
        };

        public static Dictionary<string, IapProduct> idToProducts = products.ToDictionary(_ => _.productId);

        public static Dictionary<string, IapProduct> androidIdToProducts = products.ToDictionary(_ => _.productId.ToLower());

        public static Dictionary<long, IapProduct> discordIdToProducts = products.ToDictionary(_ => _.discordId);

        public static Dictionary<uint, IapProduct> steamIdToProducts = products.ToDictionary(_ => _.steamId);

        public string name;

        public string productId;

        public int currencyReward;

        public long discordId;

        public uint steamId;

        public string priceString;

        public int steamPrice;

        public IapProduct(string name, string productId, long discordId, uint steamId, string priceString, int steamPrice, int currencyReward)
        {
            this.name = name;
            this.productId = productId;
            this.discordId = discordId;
            this.steamId = steamId;
            this.priceString = priceString;
            this.steamPrice = steamPrice;
            this.currencyReward = currencyReward;
        }
    }
}
