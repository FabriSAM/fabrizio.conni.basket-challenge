using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface I_Sound
{
    public void Play(string name);
    public void Stop(string name);
    public void SetVolume(string name, float volume);
}
