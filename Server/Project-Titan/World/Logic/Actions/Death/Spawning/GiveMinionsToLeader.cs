using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Data;
using Utils.NET.Logging;
using World.Logic.Reader;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.Death.Spawning
{
    public class GiveMinionsToLeader : DeathAction
    {
        public List<ushort> topLeaderType = new List<ushort>();

        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            switch (name)
            {
                case "ifTopLeader":
                    var topName = reader.ReadString();
                    var topInfo = GameData.GetObjectByName(topName);
                    if (topInfo == null)
                    {
                        Log.Error($"No top enemy named: {topName}");
                        return true;
                    }
                    topLeaderType.Add(topInfo.id);
                    return true;
            }
            return false;
        }

        public override void OnDeath(Enemy enemy, Player killer, List<Damager> damagers)
        {
            if (topLeaderType.Count > 0)
            {
                var topmost = enemy.GetTopmostLeader(out int count);
                if (topmost == null || !topLeaderType.Contains(topmost.info.id)) return;
                if (topmost.IsDead) return;
            }
            enemy.GiveMinionsToLeader();
        }
    }
}
