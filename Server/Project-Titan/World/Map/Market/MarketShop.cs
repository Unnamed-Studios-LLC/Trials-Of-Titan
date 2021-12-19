using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TitanCore.Core;
using Utils.NET.Collections;
using Utils.NET.Geometry;
using Utils.NET.Utils;
using World.Map.Objects.Map;

namespace World.Map.Market
{
    public struct MarketItem
    {
        public Item item;

        public long cost;

        public long premiumCost;

        public MarketItem(Item item, long cost, long premiumCost)
        {
            this.item = item;
            this.cost = cost;
            this.premiumCost = premiumCost;
        }
    }

    public class MarketShop
    {
        private static Range Market_Stand_TTL = new Range(180, 240);

        private struct MarketStand
        {
            public MarketDisplay display;

            public float expireTime;

            public MarketStand(MarketDisplay display, float expireTime)
            {
                this.display = display;
                this.expireTime = expireTime;
            }

            public bool IsExpired(ref WorldTime time)
            {
                return time.totalTime > expireTime;
            }
        }

        private List<MarketStand> liveStands = new List<MarketStand>();

        private Dictionary<Int2, MarketStand> standMap = new Dictionary<Int2, MarketStand>();

        private Queue<MarketItem> purchasables = new Queue<MarketItem>();

        private Queue<Int2> points = new Queue<Int2>();

        public MarketShop(MarketItem[] purchasables)
        {
            foreach (var item in purchasables.Randomize())
                this.purchasables.Enqueue(item);
        }

        public void AddDisplayPoints(IEnumerable<Int2> points)
        {
            if (points == null) return;
            foreach (var point in points.ToArray().Randomize())
                this.points.Enqueue(point);
        }

        public void Tick(World world, ref WorldTime time)
        {
            for (int i = 0; i < liveStands.Count; i++)
            {
                var stand = liveStands[i];
                if (stand.IsExpired(ref time))
                {
                    stand.display.RemoveFromWorld();

                    var point = stand.display.position.Value.ToInt2();
                    standMap.Remove(point);
                    liveStands.RemoveAt(i);
                    i--;

                    points.Enqueue(point);
                    purchasables.Enqueue(new MarketItem(stand.display.purchasable.Value, stand.display.cost.Value, stand.display.premiumCost.Value));
                }
            }

            while (purchasables.Count > 0)
            {
                if (points.Count == 0)
                    break;

                var purchasable = purchasables.Dequeue();
                var point = points.Dequeue();

                AddDisplay(purchasable, point, (float)time.totalTime + Market_Stand_TTL.GetRandom(), world);
            }
        }

        private void AddDisplay(MarketItem purchasable, Int2 position, float endTime, World world)
        {
            var display = new MarketDisplay(purchasable.item, purchasable.premiumCost, purchasable.cost);
            display.position.Value = position.ToVec2() + 0.5f;
            world.objects.AddObject(display);

            var stand = new MarketStand(display, endTime);
            liveStands.Add(stand);
            standMap.Add(position, stand);
        }
    }
}
