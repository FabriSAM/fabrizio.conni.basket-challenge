using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FabrizioConni.BasketChallenge.Audio
{
    [System.Serializable]
    public class Sound
    {
        public string Name;
        public AudioClip Clip;
    }

    // Audio manager class implementing the I_Sound interface
    [RequireComponent(typeof(AudioSource))]
    public class AudioMngr : MonoBehaviour, I_Sound
    {
        #region Serialized Fields
        [SerializeField]
        private AudioSource source;
        [SerializeField]
        private AudioSource sfx;
        [SerializeField]
        private List<Sound> sounds;
        #endregion

        #region Properties
        public static AudioMngr Instance;
        #endregion

        #region Monobehaviour Callbacks
        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            DontDestroyOnLoad(gameObject);
        }
        #endregion

        #region Public Methods
        public void Play(string name)
        {
            Sound s = sounds.Find(sound => sound.Name == name);
            if (s.Clip == null)
            {
                Debug.LogWarning("Clip non assegnato per il suono: " + s.Name);
            }
            if (s != null)
            {
                sfx.PlayOneShot(s.Clip);

                Debug.Log("Playing sound: " + name);
            }
        }

        public void PlayBackground()
        {
            source.Play();
        }

        public void Stop(string name)
        {
            Sound s = sounds.Find(sound => sound.Name == name);
            if (s != null) sfx.Stop();
        }

        public void StopWithFade(string name, float fadeDuration)
        {
            Sound s = sounds.Find(sound => sound.Name == name);
            if (s != null)
            {
                StartCoroutine(FadeOut(source, fadeDuration));
            }
        }

        public void SetVolume(AudioSource source, string name, float volume)
        {
            Sound s = sounds.Find(sound => sound.Name == name);
            if (s != null) source.volume = volume;
        }
        #endregion

        #region Coroutines
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
        #endregion
    }
}