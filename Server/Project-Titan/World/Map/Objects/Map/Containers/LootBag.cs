using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Data;
using TitanDatabase.Models;
using World.Map.Objects.Entities;

namespace World.Map.Objects.Map.Containers
{
    public class LootBag : Container
    {
        private static GameColor almostGoneFlashColor = new GameColor(-50, -50, -50);

        public override GameObjectType Type => GameObjectType.LootBag;

        public override bool Ticks => true;

        public float livingTime = 60;

        public float deathTime;

        public override void OnAddToWorld()
        {
            base.OnAddToWorld();

            deathTime = (float)world.time.totalTime + livingTime;
        }

        protected override void DoTick(ref WorldTime time)
        {
            base.DoTick(ref time);

            if (time.totalTime + 15 >= deathTime)
            {
                flashColor.Value = almostGoneFlashColor;
            }

            if (time.totalTime >= deathTime)
            {
                DestroyBag();
            }
            else
                RemoveIfEmpty();
        }

        private void RemoveIfEmpty()
        {
            var size = GetContainerSize();
            for (int i = 0; i < size; i++)
            {
                if (GetItem(i) != null)
                    return;
            }

            DestroyBag();
        }

        private void DestroyBag()
        {
            world.objects.RemoveObjectPostLogic(this);
        }

        public override void OnRemoveFromWorld()
        {
            base.OnRemoveFromWorld();

            DestroyItems();
        }

        private async void DestroyItems()
        {
            for (int i = 0; i < GetContainerSize(); i++)
            {
                var item = GetItem(i);
                if (item == null) continue;
                await ServerItem.Delete(item.id);
            }
        }
    }
}
