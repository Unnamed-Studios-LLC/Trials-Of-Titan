using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class ActionsPopup : MonoBehaviour
{
    public World world;

    public ClassPreview preview;

    public TextMeshProUGUI playerNameLabel;

    private Character character;

    private bool didDown;

    public void Show(Character character, Vector3 position)
    {
        transform.position = position - new Vector3(5, 0, 0);

        didDown = false;
        this.character = character;
        preview.SetClass(character.GetSkinInfo());
        playerNameLabel.text = character.playerName;
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        character = null;
        gameObject.SetActive(false);
    }

    public void Teleport()
    {
        world.Teleport(character);
        Hide();
    }

    public void Trade()
    {
        world.Trade(character);
        Hide();
    }

    public void Message()
    {

        Hide();
    }

    private void LateUpdate()
    {
        if (!didDown)
        {
            didDown = Input.GetMouseButtonDown(0);
        }

        if (didDown && Input.GetMouseButtonUp(0))
        {
            gameObject.SetActive(false);
        }
    }
}