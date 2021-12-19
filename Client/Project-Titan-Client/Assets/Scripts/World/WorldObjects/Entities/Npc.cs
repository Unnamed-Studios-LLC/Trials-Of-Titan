using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Data;

public class Npc : NotPlayable
{
    public override GameObjectType ObjectType => GameObjectType.Npc;

    protected override bool Ally => true;


}
