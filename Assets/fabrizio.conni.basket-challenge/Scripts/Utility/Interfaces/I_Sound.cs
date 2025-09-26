using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface I_Sound
{
    abstract public void Play(string name);
    abstract public void SetVolume(AudioSource source, float volume);
}
