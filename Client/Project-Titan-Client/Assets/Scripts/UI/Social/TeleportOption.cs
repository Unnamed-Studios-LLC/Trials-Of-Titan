using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Net.Packets.Client;
using TMPro;
using UnityEngine;

public class TeleportOption : MonoBehaviour
{
    public World world;

    public ClassPreview preview;

    public TextMeshProUGUI nameLabel;

    private Character character;

    public void Setup(Character character)
    {
        if (character == null)
        {
            gameObject.SetActive(false);
            this.character = null;
            return;
        }

        this.character = character;
        gameObject.SetActive(true);

        preview.SetClass(character.GetSkinInfo());
        nameLabel.text = character.playerName;
    }

    public void Select()
    {
        world.Teleport(character);
    }
}
