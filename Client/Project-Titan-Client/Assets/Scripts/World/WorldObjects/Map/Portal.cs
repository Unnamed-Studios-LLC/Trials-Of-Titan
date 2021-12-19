using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Data;
using TitanCore.Net.Packets.Client;
using TitanCore.Net.Packets.Models;
using UnityEngine;

namespace Assets.Scripts.World.WorldObjects.Map
{
    public class Portal : SpriteWorldObject, IInteractable
    {
        public override GameObjectType ObjectType => GameObjectType.Portal;

        public string[] InteractionOptions => new string[] { "Enter" };

        public string InteractionTitle => worldName;

        private string worldName = "";

        public override void LoadObjectInfo(GameObjectInfo info)
        {
            base.LoadObjectInfo(info);

            name = info.name;

            var texture = info.textures[UnityEngine.Random.Range(0, info.textures.Length)];
            SetSprite(TextureManager.GetSprite(texture.displaySprite));
        }

        public override void Enable()
        {
            base.Enable();

            transform.localEulerAngles = new Vector3(0, 0, 0);
        }

        protected override void ProcessStat(NetStat stat, bool first)
        {
            base.ProcessStat(stat, first);

            switch (stat.type)
            {
                case ObjectStatType.Name:
                    worldName = (string)stat.value;
                    ShowGroundLabel(worldName);
                    break;
            }
        }

        public void Interact(int option)
        {
            world.gameManager.client.SendAsync(new TnInteract(world.clientTickId, gameId, ((Vector2)world.player.Position).ToVec2(), 0));
        }

        public void OnEnter()
        {

        }

        public void OnExit()
        {

        }
    }
}