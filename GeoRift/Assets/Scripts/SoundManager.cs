using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    public float MasterVolume { get; set; } = 1f;

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip[] musicClips;

    private int _currentClipIndex = 0;
    private Coroutine _musicCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        StartMusic();
    }
    
    public void UpdateVolume(float volume)
    {
        MasterVolume = volume;
        musicSource.volume = MasterVolume;
    }

    public void PlayClip(AudioClip clip, float pitch = 1f)
    {
        GameObject obj = new GameObject($"AudioOneShot_{clip.name}");
        AudioSource source = obj.AddComponent<AudioSource>();

        source.clip = clip;
        source.volume = MasterVolume;
        source.pitch = pitch;
        source.Play();

        Destroy(obj, clip.length / Mathf.Abs(pitch));
    }

    public void StartMusic()
    {
        if (_musicCoroutine != null)
            StopCoroutine(_musicCoroutine);

        _musicCoroutine = StartCoroutine(MusicLoop());
    }

    public void StopMusic()
    {
        if (_musicCoroutine != null)
            StopCoroutine(_musicCoroutine);

        musicSource.Stop();
    }

    private IEnumerator MusicLoop()
    {
        while (true)
        {
            AudioClip clip = musicClips[_currentClipIndex];
            musicSource.clip = clip;
            musicSource.Play();

            yield return new WaitForSeconds(clip.length);

            _currentClipIndex = (_currentClipIndex + 1) % musicClips.Length;
        }
    }
}
