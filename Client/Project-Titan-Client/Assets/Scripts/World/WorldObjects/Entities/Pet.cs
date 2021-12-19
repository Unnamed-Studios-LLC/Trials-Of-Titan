using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Core;
using TitanCore.Data;
using TitanCore.Net.Packets.Models;

public class Pet : Npc, IContainer
{
    public override GameObjectType ObjectType => GameObjectType.Pet;

    private uint ownerId;

    private bool isPlayers = false;

    private Option allyPetTransparency;

    protected override void Awake()
    {
        base.Awake();

        allyPetTransparency = Options.Get(OptionType.AllyPetTransparency);
    }

    public override void Enable()
    {
        base.Enable();

        isPlayers = false;

        allyPetTransparency.AddFloatCallback(OnAllyPetTransparency);
        OnAllyPetTransparency(allyPetTransparency.GetFloat());
    }

    public override void Disable()
    {
        base.Disable();

        allyPetTransparency.RemoveFloatCallback(OnAllyPetTransparency);
    }

    private void OnAllyPetTransparency(float value)
    {
        if (isPlayers) return;
        alpha = value / 10f;
    }

    protected override void ProcessStat(NetStat stat, bool first)
    {
        base.ProcessStat(stat, first);

        switch (stat.type)
        {
            case ObjectStatType.OwnerId:
                ownerId = (uint)stat.value;
                break;
        }
    }

    public override void WorldFixedUpdate(uint time, uint delta)
    {
        base.WorldFixedUpdate(time, delta);

        if (world.player != null)
        {
            isPlayers = world.player.gameId == ownerId;
            if (isPlayers)
                alpha = 1;
        }
    }

    public override bool ShowLootMenu()
    {
        return isPlayers;
    }

    public override Item GetItem(int index)
    {
        if (isPlayers)
        {
            var backpack = world.player.backpack;
            if (index < backpack.Length)
                return backpack[index];
        }
        return base.GetItem(index);
    }

    public override void SetItem(int index, Item item)
    {
        if (isPlayers)
        {
            var backpack = world.player.backpack;
            if (index < backpack.Length)
            {
                backpack[index] = item;
                return;
            }
        }
        base.SetItem(index, item);
    }

}
