using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioMixer mixer;

    public AudioSource sfxSource;
    public AudioSource musicSource;

    public SoundLibrary soundLibrary;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (sfxSource == null || musicSource == null)
        {
            AudioSource[] sources = GetComponentsInChildren<AudioSource>();

            foreach (var src in sources)
            {
                if (src.loop)
                    musicSource = src;
                else
                    sfxSource = src;
            }
        }
    }


    public void PlaySFX(AudioClip clip, float volume = 1f, float pitchVariance = 0.05f)
    {
        if (clip == null) return;

        sfxSource.pitch = 1f + Random.Range(-pitchVariance, pitchVariance);
        sfxSource.PlayOneShot(clip, volume);
        sfxSource.pitch = 1f;
    }

    public void PlayRandomSFX(AudioClip[] clips, float volume = 1f)
    {
        if (clips == null || clips.Length == 0) return;
        PlaySFX(clips[Random.Range(0, clips.Length)], volume);
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (musicSource.clip == clip) return;

        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }
}

