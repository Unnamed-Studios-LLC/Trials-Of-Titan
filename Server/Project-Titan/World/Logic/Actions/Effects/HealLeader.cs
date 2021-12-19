using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Net.Packets.Server;
using World.Logic.Components;
using World.Logic.Reader;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.Effects
{
    public class HealLeaderValue
    {
        public object cooldownValue;
    }

    public class HealLeader : LogicAction<HealLeaderValue>
    {
        public int amount;

        public float maxPercent = 1;

        private Cooldown cooldown;

        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            switch (name)
            {
                case "amount":
                    amount = reader.ReadInt();
                    return true;
                case "maxPercent":
                    maxPercent = reader.ReadFloat();
                    return true;
            }
            if (cooldown.ReadParameterValue(name, reader))
                return true;
            return false;
        }

        public override void Init(Entity entity, out HealLeaderValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new HealLeaderValue();
            cooldown.Init(out obj.cooldownValue);
        }

        public override void Tick(Entity entity, ref HealLeaderValue obj, ref StateContext context, ref WorldTime time)
        {
            if (!(entity is Enemy enemy)) return;
            if (cooldown.Tick(ref obj.cooldownValue, ref time))
            {
                if (enemy.leader == null) return;
                var perc = enemy.leader.maxHealth.Value * maxPercent;
                if (enemy.leader.GetHealth() >= perc) return;

                enemy.leader.Heal(amount);
                var pkt = new TnPlayEffect(new HealLaserWorldEffect(enemy.gameId, enemy.leader.gameId));
                foreach (var player in enemy.playersSentTo)
                    player.client.SendAsync(pkt);
            }
        }
    }
}
