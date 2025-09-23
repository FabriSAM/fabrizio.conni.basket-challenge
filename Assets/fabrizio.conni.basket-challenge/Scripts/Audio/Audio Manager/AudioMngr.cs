using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static AudioMngr;

public class AudioMngr : MonoBehaviour, I_Sound
{
    [SerializeField]
    private AudioSource source;
    [SerializeField]
    private AudioSource sfx;

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
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public void Play(string name)
    {
        Sound s = sounds.Find(sound => sound.name == name);
        if (s.clip == null)
        {
            Debug.LogWarning("Clip non assegnato per il suono: " + s.name);
        }
        if (s != null)
        {
            sfx.PlayOneShot(s.clip);
            
            Debug.Log("Playing sound: " + name);
        }
    }

    public void PlayBackground()
    {
        source.Play();
    }
    public void Stop(string name)
    {
        Sound s = sounds.Find(sound => sound.name == name);
        if (s != null) sfx.Stop();
    }

    public void StopWithFade(string name, float fadeDuration)
    {
        Sound s = sounds.Find(sound => sound.name == name);
        if (s != null)
        {
            StartCoroutine(FadeOut(source, fadeDuration));
        }
    }

    public void SetVolume(AudioSource source, string name, float volume)
    {
        Sound s = sounds.Find(sound => sound.name == name);
        if (s != null) source.volume = volume;
    }
    private IEnumerator FadeOut(AudioSource audioSource, float fadeDuration)
    {
        float startVolume = audioSource.volume;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, time / fadeDuration);
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.Stop();
        audioSource.volume = startVolume;
    }

}
