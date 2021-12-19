using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TitanCore.Core;
using TitanCore.Data;
using TitanCore.Data.Items;
using TitanCore.Files;
using Utils.NET.Geometry;
using Utils.NET.Utils;
using World.Map.Market;
using World.Map.Objects.Map;
using World.Worlds.Gates;

namespace World.Worlds
{
    public class Nexus : World
    {
        #region Market Items

        #region Packages

        private static MarketItem[] packageItems = new MarketItem[]
        {
            new MarketItem(new Item(0x2a9, true, 1), 0, 1000), // beginner's pack
        };

        #endregion

        #region Emotes

        private static MarketItem[] emoteItems = new MarketItem[]
        {
            new MarketItem(new Item(0x2000, true, 1), 1000, 150), // f
            new MarketItem(new Item(0x2001, true, 1), 1000, 150), // heart
            new MarketItem(new Item(0x2002, true, 1), 1000, 150), // danger
            new MarketItem(new Item(0x2003, true, 1), 1000, 150), // thumbs up
            new MarketItem(new Item(0x2004, true, 1), 1000, 150), // rare chest
        };

        #endregion

        #region Pet Totems

        private static MarketItem[] companionItems = new MarketItem[]
        {
            new MarketItem(new Item(0x3003, true, 1), 3500, 300), // snail companion
            new MarketItem(new Item(0x3001, true, 1), 4000, 400), // hula companion
            new MarketItem(new Item(0x3005, true, 1), 4500, 500), // mammoth companion
            new MarketItem(new Item(0x3007, true, 1), 4000, 400), // tortoise companion
        };

        #endregion

        #region Antiques

        private static MarketItem[] antiqueItems = new MarketItem[]
        {
            new MarketItem(new Item(0x2a6, true, 3), 0, 35), // health vial
            new MarketItem(new Item(0x2a7, true, 1), 25, 15), // healing spell
            //new MarketItem(new Item(0x2aa, false, 1), 12000, 0), // soulless ring
            new MarketItem(new Item(0x2ab, false, 1), 8000, 0), // soulless robe
            new MarketItem(new Item(0x2ac, false, 1), 8000, 0), // soulless heavy armor
            new MarketItem(new Item(0x2ad, false, 1), 8000, 0), // soulless light armor
            /*
            new MarketItem(new Item("Scroll of Agility", true, 1), 0, 0),
            new MarketItem(new Item("Scroll of Power", true, 1), 0, 0),
            new MarketItem(new Item("Scroll of Fortitude", true, 1), 0, 0),
            new MarketItem(new Item("Scroll of Stamina", true, 1), 0, 0),
            new MarketItem(new Item("Scroll of Life", true, 1), 0, 0),
            */
        };

        #endregion

        #region Skins

        private static MarketItem[] skinItems = new MarketItem[]
        {
            new MarketItem(new Item(0x5000, true, 1), 0, 850), // dragon warrior
            new MarketItem(new Item(0x5001, true, 1), 0, 850), // mist dancer
            new MarketItem(new Item(0x5002, true, 1), 0, 850), // soul hunter
            new MarketItem(new Item(0x5003, true, 1), 0, 850), // plague doctor
            new MarketItem(new Item(0x5004, true, 1), 0, 850), // prince of sands
            new MarketItem(new Item(0x5005, true, 1), 2000, 300), // baldweaver
            new MarketItem(new Item(0x5006, true, 1), 0, 850), // mannah's guard
            new MarketItem(new Item(0x5007, true, 1), 0, 1600), // mythic brotherhood
            new MarketItem(new Item(0x5008, true, 1), 0, 850), // crusading commander
            new MarketItem(new Item(0x5009, true, 1), 0, 850), // black lancer
            new MarketItem(new Item(0x500a, true, 1), 0, 850), // crusading minister
            new MarketItem(new Item(0x500b, true, 1), 0, 1600), // master of the order
        };

        #endregion

        #endregion

        public override bool LimitSight => false;

        protected override string MapFile => "nexus.mef";

        public override string WorldName => "Nexus";

        public override bool KeyedAccess => false;

        private List<MarketShop> marketShops = new List<MarketShop>();

        private List<Int2> portalPositions = new List<Int2>();

        public override int MaxPlayerCount => 200;

        public Portal AddOverworldPortal(string name, string remoteServer, uint worldId)
        {
            var info = GameData.objects[0xa22];

            var portal = new Portal(remoteServer, worldId);
            portal.worldName.Value = name;
            portal.Initialize(info);

            int portalPositionIndex = Rand.Next(portalPositions.Count);
            portal.position.Value = portalPositions[portalPositionIndex].ToVec2() + 0.5f;
            portalPositions.RemoveAt(portalPositionIndex);

            objects.AddObject(portal);
            return portal;
        }

        public void AddPortal(World world)
        {
            var info = GameData.objects[world.PreferredPortal];

            var portal = new Portal(world.worldId);
            portal.worldName.Value = world.WorldName;
            portal.Initialize(info);

            int portalPositionIndex = Rand.Next(portalPositions.Count);
            portal.position.Value = portalPositions[portalPositionIndex].ToVec2() + 0.5f;
            portalPositions.RemoveAt(portalPositionIndex);

            objects.AddObject(portal);
        }

        public void ReturnPortalPosition(Int2 position)
        {
            portalPositions.Add(position);
        }

        protected override void DoInitWorld()
        {
            base.DoInitWorld();

            portalPositions = new List<Int2>(GetRegions(Region.Portal));

            CreateMarketShop(GetRegions(Region.Shop1), skinItems);
            CreateMarketShop(GetRegions(Region.Shop2), companionItems);
            CreateMarketShop(GetRegions(Region.Shop3), emoteItems);
            CreateMarketShop(GetRegions(Region.Shop4), antiqueItems);
            CreateMarketShop(GetRegions(Region.Tag1), packageItems);
        }

        private void CreateMarketShop(IEnumerable<Int2> points, params MarketItem[] items)
        {
            var shop = new MarketShop(items);
            shop.AddDisplayPoints(points);
            marketShops.Add(shop);
        }

        public override void Tick()
        {
            foreach (var shop in marketShops)
                shop.Tick(this, ref time);

            base.Tick();
        }
    }
}
