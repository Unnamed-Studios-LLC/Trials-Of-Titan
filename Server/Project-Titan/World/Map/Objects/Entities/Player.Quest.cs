using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Net.Packets.Server;

namespace World.Map.Objects.Entities
{
    public partial class Player
    {
        public GameObject quest;

        private int questDamage = 0;

        public void SetQuest(GameObject obj)
        {
            if (quest == obj) return;
            quest = obj;

            if (quest != null)
            {
                if (quest is Enemy questEnemy)
                    questDamage = questEnemy.GetDamageBy(client.account.id);
                client.SendAsync(new TnQuest(obj.gameId, obj.info.id));
            }
            else
            {
                client.SendAsync(new TnQuest(0, 0));
                ClearQuest();
            }
        }

        public void ProcessQuest(ref WorldTime time)
        {
            if (quest == null) return;
            gameState.ProcessObject(quest, ref time);
        }

        public void AddQuestDamage(int damage)
        {
            if (quest == null) return;
            //questDamage += damage;
            //client.SendAsync(new TnQuestDamage(questDamage));
        }

        private void TickQuest(ref WorldTime time)
        {
            if (quest != null)
            {
                var questEnemy = quest as Enemy;
                if (questEnemy != null)
                {
                    var dmg = questEnemy.GetDamageBy(client.account.id);
                    if (dmg != questDamage)
                    {
                        questDamage = dmg;
                        client.SendAsync(new TnQuestDamage(questDamage));
                    }
                }

                if ((questEnemy != null && questEnemy.IsDead) || quest.world == null)
                    SetQuest(null);
                else
                    quest.Tick(ref time);
            }

            if (time.tickId % (WorldManager.Ticks_Per_Second * 5) != 0) return;
            world.AssignQuest(this);
        }

        private void ClearQuest()
        {
            quest = null;
            questDamage = 0;
        }
    }
}
