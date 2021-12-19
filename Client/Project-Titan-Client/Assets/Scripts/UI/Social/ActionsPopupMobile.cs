using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ActionsPopupMobile : MonoBehaviour
{
    public TextMeshProUGUI nameLabel;

    public RectTransform rectTransform;

    public World world;

    private Character character;

    public void Show(Character character, Vector2 position)
    {
        this.character = character;
        nameLabel.text = character.playerName;

        var height = rectTransform.rect.height;
        if (position.y - height < 0)
            position.y += height;

        rectTransform.position = position;

        gameObject.SetActive(true);
    }

    private void Hide()
    {
        character = null;
        gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            var touch = Input.GetTouch(i);
            if (touch.phase == TouchPhase.Began && !WithinRect(touch.position))
                Hide();
        }

        if (Input.GetMouseButtonDown(0) && !WithinRect(Input.mousePosition))
            Hide();
    }

    private bool WithinRect(Vector2 position)
    {
        var rect = rectTransform.rect;
        rect.position = (Vector2)rectTransform.position - rect.size * rectTransform.pivot;

        return rect.Contains(position);
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
}
