using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TeleportPanel : MonoBehaviour
{
    public World world;

    public TeleportOption[] options;

    private bool didDown;

    public void Show(Vector2 worldPosition, float sampleRadius)
    {
        var characters = world.characters.Where(_ => _ != world.player && (((Vector2)_.transform.position) - worldPosition).magnitude <= sampleRadius).OrderBy(_ => (worldPosition - ((Vector2)_.transform.position)).sqrMagnitude).ToArray();
        if (characters.Length == 0)
        {
            return;
        }

        didDown = false;
        for (int i = 0; i < options.Length; i++)
        {
            var option = options[i];
            if (i < characters.Length)
                option.Setup(characters[i]);
            else
                option.Setup(null);
        }

        gameObject.SetActive(true);
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
