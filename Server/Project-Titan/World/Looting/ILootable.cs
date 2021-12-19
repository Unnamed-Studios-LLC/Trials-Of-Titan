using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using World.Map.Objects.Entities;

namespace World.Looting
{
    public interface ILootable
    {
        void AddItems(List<Item> items, PlayerLootVariables variables);
    }
}
