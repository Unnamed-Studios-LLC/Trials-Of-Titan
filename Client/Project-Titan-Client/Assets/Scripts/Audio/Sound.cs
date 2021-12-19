using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class Sound
{
    public AudioClip clip;

    [Range(0, 1)]
    public float volume = 1;

    [Range(0, 3)]
    public float pitch = 1;
}
