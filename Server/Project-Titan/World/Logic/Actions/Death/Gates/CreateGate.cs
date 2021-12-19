using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Data;
using Utils.NET.Geometry;
using World.Logic.Reader;
using World.Map.Objects.Entities;
using World.Map.Objects.Map;
using World.Worlds.Gates;

namespace World.Logic.Actions.Death.Gates
{
    public class CreateGate : DeathAction
    {
        private Type gateType;

        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            switch (name)
            {
                case "type":
                    switch (reader.ReadString())
                    {
                        case "forge":
                            gateType = typeof(ValdoksForge);
                            break;
                        case "dumir":
                            gateType = typeof(Dumir);
                            break;
                        case "bubra":
                            gateType = typeof(BhogninsGate);
                            break;
                        case "woods":
                            gateType = typeof(RictornsGate);
                            break;
                        case "fortress":
                            gateType = typeof(MannahsFortress);
                            break;
                    }
                    return true;
            }
            return false;
        }

        public override void OnDeath(Enemy enemy, Player killer, List<Damager> damagers)
        {
            DoGate(enemy.world, gateType, enemy.position.Value);
        }

        private Gate DoGate(World world, Type type, Vec2 position)
        {
            if (type == null) return null;
            var gate = (Gate)Activator.CreateInstance(type);
            gate.worldId = world.manager.GetWorldId();
            AddGate(world, gate, position);
            return gate;
        }

        private async void AddGate(World world, Gate gate, Vec2 position)
        {
            await Task.Run(() =>
            {
                gate.InitWorld();
            });
            world.manager.AddWorld(gate);

            var portal = new Portal(gate.worldId);
            portal.worldName.Value = gate.WorldName;
            portal.position.Value = position;
            portal.Initialize(GameData.objects[gate.PreferredPortal]);

            gate.portal = portal;
            world.PushTickAction(() =>
            {
                world.objects.AddObject(portal);
            });
        }
    }
}
