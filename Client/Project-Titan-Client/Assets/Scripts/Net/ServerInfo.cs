using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ServerInfo
{
    public static bool HasDefault => !string.IsNullOrWhiteSpace(PlayerPrefs.GetString("SelectedServerName", ""));

    public static ServerInfo GetSelected()
    {
        return new ServerInfo
        {
            name = PlayerPrefs.GetString("SelectedServerName", ""),
            host = PlayerPrefs.GetString("SelectedServerHost", "")
        };
    }

    public string name;

    public string host;
}
