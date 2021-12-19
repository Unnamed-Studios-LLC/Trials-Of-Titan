using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Net.Packets.Client;
using TitanCore.Net.Packets.Server;

public class TradeRequest : IRequest
{
    private World world;

    private string fromPlayer;

    public TradeRequest(World world, TnTradeRequest request)
    {
        this.world = world;
        fromPlayer = request.fromPlayer;
    }

    public void OnAccept()
    {
        world.gameManager.client.SendAsync(new TnChat($"/trade {fromPlayer}"));
    }

    public void OnDecline()
    {

    }
}
