using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Data;
using TitanCore.Data.Entities;
using Utils.NET.Geometry;

namespace World.Map.Objects.Entities
{
    public partial class Player
    {
        public Pet pet;

        public void LoadPet()
        {
            if (character.pet == 0) return;

            var petInfo = GameData.objects[character.pet];
            var newPet = new Pet(client.account.id, gameId);
            newPet.Initialize(petInfo);
            newPet.position.Value = position.Value + new Vec2(0, -0.2f);
            AddStats(newPet.inventory.stats);

            if (pet != null)
            {
                SavePetInventory();
                RemoveStats(pet.inventory.stats);
                RemovePet();
            }

            pet = newPet;
            LoadPetInventory();
            world.objects.SpawnObject(newPet);
        }

        private void LoadPetInventory()
        {
            for (int i = 0; i < pet.inventory.Length; i++)
            {
                if (character.items.Count == 12 + i)
                {
                    character.items.Add(null);
                    character.itemIds.Add(0);
                }

                pet.inventory.SetItem(i, character.items[12 + i]);
            }
        }

        private void SavePetInventory()
        {
            if (pet == null) return;
            for (int i = 0; 12 + i < character.items.Count && i < pet.inventory.Length; i++)
            {
                var item = pet.inventory.GetItem(i);
                if (item == null)
                {
                    character.items[12 + i] = null;
                    character.itemIds[12 + i] = 0;
                }
                else
                {
                    item.containerId = character.id;
                    character.items[12 + i] = item;
                    character.itemIds[12 + i] = item.id;
                }
            }
        }

        private void RemovePet()
        {
            if (pet == null) return;
            SavePetInventory();
            pet.RemoveFromWorld();
            pet = null;
        }

        public void ProcessPet(ref WorldTime time)
        {
            if (pet == null) return;
            gameState.ProcessObject(pet, ref time);
        }

        private void TickPet(ref WorldTime time)
        {
            if (pet == null || pet.world == null) return;
            pet.Tick(ref time);
        }
    }
}
