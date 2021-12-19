using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Net.Packets.Client;
using World.Map.Objects.Entities;

namespace World.Map.Objects.Interfaces
{
    public interface IInteractable
    {
        void Interact(Player player, TnInteract interact);
    }
}
