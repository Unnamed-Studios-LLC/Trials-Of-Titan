using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Data;
using TitanCore.Net.Packets.Client;
using TitanCore.Net.Packets.Models;
using UnityEngine;

public class NomadCharm : SpriteWorldObject
{
    public override GameObjectType ObjectType => GameObjectType.NomadCharm;

    protected override bool HasShadow => false;

    protected override bool IsBillboard => false;

    private bool consumed = false;

    public static HashSet<uint> consumedCharms = new HashSet<uint>();

    public override void Enable()
    {
        base.Enable();

        consumed = false;

        transform.localEulerAngles = new Vector3(0, 0, 0);
    }

    public override void NetUpdate(NetStat[] stats, bool first)
    {
        base.NetUpdate(stats, first);

        if (consumedCharms.Contains(gameId))
        {
            Consume();
        }
    }

    public override void WorldFixedUpdate(uint time, uint delta)
    {
        base.WorldFixedUpdate(time, delta);

        if (consumed) return;
        if (Vector2.Distance(world.player.Position, Position) < 1)
        {
            world.gameManager.client.SendAsync(new TnInteract(world.clientTickId, gameId, ((Vector2)world.player.Position).ToVec2(), 0));
            Consume();
        }
    }

    private void Consume()
    {
        consumedCharms.Add(gameId);
        SetTexture(1);
        consumed = true;
    }

    protected override void SetOutlineGlow(WorldDrawStyle style)
    {

    }
}
