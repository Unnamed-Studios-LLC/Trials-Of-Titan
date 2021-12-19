using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Data.Entities;
using Utils.NET.Utils;
using World.Logic.Components;
using World.Logic.Reader;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.Social
{
    public class TauntValue
    {
        public object cooldownValue;

        public Dictionary<string, string> chatValues = new Dictionary<string, string>();
    }

    public class Taunt : LogicAction<TauntValue>
    {
        private List<TemplateString> texts = new List<TemplateString>();

        private Cooldown cooldown = new Cooldown();

        private Dictionary<string, string> chatValues = new Dictionary<string, string>();

        private float searchRadius = 8;

        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            switch (name)
            {
                case "text":
                    texts.Add(new TemplateString(reader.ReadString(), '#'));
                    return true;
                case "searchRadius":
                    searchRadius = reader.ReadFloat();
                    break;
            }
            if (cooldown.ReadParameterValue(name, reader))
                return true;
            return false;
        }

        public override void Init(Entity enemy, out TauntValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new TauntValue();
            cooldown.Init(out obj.cooldownValue);

            obj.chatValues["name"] = enemy.info.name;
            if (enemy.info is EnemyInfo enemyInfo)
            {
                if (!string.IsNullOrWhiteSpace(enemyInfo.title))
                    obj.chatValues["name"] = enemyInfo.title;
            }
        }

        public override void Tick(Entity entity, ref TauntValue obj, ref StateContext context, ref WorldTime time)
        {
            if (texts.Count == 0 || !(entity is NotPlayable notPlayable)) return;
            if (cooldown.Tick(ref obj.cooldownValue, ref time))
            {
                if (texts.Count > 1)
                    notPlayable.Chat(texts[Rand.Next(texts.Count)].Build(BuildValues(entity, obj.chatValues)));
                else
                    notPlayable.Chat(texts[0].Build(BuildValues(entity, obj.chatValues)));
            }
        }

        private Dictionary<string, string> BuildValues(Entity entity, Dictionary<string, string> chatValues)
        {
            if (entity is Enemy enemy)
            {
                chatValues["hp"] = enemy.GetHealth().ToString();
            }

            //var closest = entity.GetClosestPlayer(searchRadius);
            Player closest = null;
            float distance = float.MaxValue;
            foreach (var player in entity.playersSentTo)
            {
                var sqrDis = player.position.Value.SqrDistanceTo(entity.position.Value);
                if (sqrDis < distance)
                {
                    distance = sqrDis;
                    closest = player;
                }
            }

            if (closest != null)
                chatValues["player"] = closest.playerName.Value;
            else
                chatValues["player"] = "player";
            return chatValues;
        }
    }
}
