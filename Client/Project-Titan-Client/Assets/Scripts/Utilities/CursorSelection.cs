using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorSelection : MonoBehaviour
{
    private Option cursorStyle;

    public Texture2D[] cursors;

    private void Awake()
    {
        cursorStyle = Options.Get(OptionType.CursorStyle);
        cursorStyle.AddStringCallback(UpdateCursor);

        UpdateCursor(cursorStyle.GetString());
    }

    private void UpdateCursor(string value)
    {
        foreach (var cursor in cursors)
        {
            if (!cursor.name.Equals(value, StringComparison.Ordinal)) continue;
            Cursor.SetCursor(cursor, new Vector2(cursor.width / 2, cursor.height / 2), CursorMode.Auto);
        }
    }
}
