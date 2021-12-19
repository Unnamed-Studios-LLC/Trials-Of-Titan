using System;
using System.Collections.Generic;
using System.Text;
using World.Logic.Reader;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.Death
{
    public class RunLogicMethod : DeathAction
    {
        private string methodKey;

        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            switch (name)
            {
                case "name":
                    methodKey = reader.ReadString();
                    return true;
            }
            return false;
        }

        public override void OnDeath(Enemy enemy, Player killer, List<Damager> damagers)
        {
            enemy.world.RunLogicMethod(methodKey, enemy);
        }
    }
}
