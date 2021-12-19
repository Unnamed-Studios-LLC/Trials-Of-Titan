using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Net.Packets.Models;
using TitanDatabase.Models;

namespace World.GameState
{
    public class Inventory
    {
        public int Length => items.Length;

        public ServerItem[] items;

        public ObjectStat<Item>[] stats;

        public Inventory(int size, ObjectStatType startStat = ObjectStatType.Inventory0, int publicCount = 4)
        {
            items = new ServerItem[size];
            stats = new ObjectStat<Item>[size];
            for (int i = 0; i < size; i++)
            {
                stats[i] = new ObjectStat<Item>((ObjectStatType)((int)startStat + i), i < publicCount ? ObjectStatScope.Public : ObjectStatScope.Private, Item.Blank, Item.Blank);
            }
        }

        public ServerItem GetItem(int slot)
        {
            if (slot >= stats.Length) return null;
            return items[slot];
        }

        public void SetItem(int slot, ServerItem item)
        {
            if (slot >= stats.Length) return;
            items[slot] = item;
            stats[slot].Value = item == null ? Item.Blank : item.itemData;
        }
    }
}
