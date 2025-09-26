using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace FabrizioConni.BasketChallenge.Audio
{


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

        private Coroutine fadecoroutine;

        [System.Serializable]
        public class Sound
        {
            public string Name;
            public AudioClip Clip;
        }

        #region Private Fields
        private Dictionary<string, AudioClip> soundDictionary;
        #endregion

        #region Properties
        public static AudioMngr Instance;
        #endregion

        #region Monobehaviour Callbacks
        private void Start()
        {
            soundDictionary = new Dictionary<string, AudioClip>();
            foreach (var sound in sounds)
            {
                if (!soundDictionary.ContainsKey(sound.Name))
                {
                    soundDictionary.Add(sound.Name, sound.Clip);
                }
            }
        }
        
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
            if (!soundDictionary.ContainsKey(name))
            {
                return;
            }
            sfx.PlayOneShot(soundDictionary[name]);
        }

        public void PlayBackground()
        {
            source.Play();
        }

        public void StopWithFade(string name, float fadeDuration)
        {
            if (fadecoroutine != null)
            {
                StopCoroutine(fadecoroutine);
                fadecoroutine = null;
            }
            fadecoroutine = StartCoroutine(FadeOut(source, fadeDuration));
        }

        public void SetVolume(AudioSource source, float volume)
        {
            source.volume = volume;
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

            Debug.Log("Audio Stopped");
            audioSource.volume = 0f;
            audioSource.Stop();
            audioSource.volume = startVolume;
            StopCoroutine(fadecoroutine);
        }
        #endregion
    }
}