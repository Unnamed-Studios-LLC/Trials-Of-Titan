using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TitanCore.Data;
using Utils.NET.Logging;
using World.Logic.Reader;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.Death.Spawning
{
    public class ChainKill : DeathAction
    {
        private HashSet<ushort> types = new HashSet<ushort>();

        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            switch (name)
            {
                case "name":

                    var typeName = reader.ReadString();
                    var info = GameData.GetObjectByName(typeName);

                    if (info == null)
                        Log.Error("No object named: " + typeName);
                    else
                        types.Add(info.id);

                    return true;
            }
            return false;
        }

        public override void OnDeath(Enemy enemy, Player killer, List<Damager> damagers)
        {
            var leaders = enemy.GetLeaderHierarchy().ToArray();
            var minions = enemy.GetMinions().ToArray();

            foreach (var minion in minions)
                TryKill(minion, killer);

            foreach (var leader in leaders)
                TryKill(leader, killer);
        }

        private void TryKill(Enemy enemy, Player killer)
        {
            if (!types.Contains(enemy.info.id)) return;
            enemy.Die(killer);
        }
    }
}
