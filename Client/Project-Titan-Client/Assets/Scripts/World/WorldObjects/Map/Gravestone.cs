using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Data;
using TitanCore.Net.Packets.Client;
using TitanCore.Net.Packets.Models;
using UnityEngine;

public class Gravestone : StaticObject, IInteractable
{
    public override GameObjectType ObjectType => GameObjectType.Gravestone;

    public string[] InteractionOptions => new string[] { "Honor" };

    public string InteractionTitle => playerName;

    private string playerName = "";

    public void Interact(int option)
    {
        world.gameManager.client.SendAsync(new TnInteract(world.clientTickId, gameId, ((Vector2)world.player.Position).ToVec2(), 0));
    }

    protected override void ProcessStat(NetStat stat, bool first)
    {
        base.ProcessStat(stat, first);

        switch (stat.type)
        {
            case ObjectStatType.Name:
                playerName = (string)stat.value;
                ShowGroundLabel(playerName);
                break;
        }
    }

    public void OnEnter()
    {

    }

    public void OnExit()
    {

    }
}
