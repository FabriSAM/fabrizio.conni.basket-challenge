using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AudioMngr;

public class AudioMngr : MonoBehaviour, I_Sound
{
    public static AudioMngr Instance;
    public List<Sound> sounds;

    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.1f, 3f)] public float pitch = 1f;
        public bool loop;
        [HideInInspector] public AudioSource source;
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    public void Play(string name)
    {
        Debug.Log("Play sound: " + name);



        Sound s = sounds.Find(sound => sound.name == name);
        if (s != null) s.source.Play();

        Debug.Log("Clip assegnato: " + s.clip);
        Debug.Log("Volume: " + s.source.volume);
        Debug.Log("Pitch: " + s.source.pitch);

        AudioSource.PlayClipAtPoint(s.clip, transform.position);

    }

    public void Stop(string name)
    {
        Sound s = sounds.Find(sound => sound.name == name);
        if (s != null) s.source.Stop();
    }

    public void SetVolume(string name, float volume)
    {
        Sound s = sounds.Find(sound => sound.name == name);
        if (s != null) s.source.volume = volume;
    }
}
