using UnityEngine;
using System.Collections;
using System;

[System.Serializable]
public struct MusicTrack
{
    public AudioClip clip;
    [Range(0f, 1f)] public float volume;
}
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    public float MasterVolume { get; set; } = 0.25f;

    [SerializeField] private AudioSource musicSource;

    [SerializeField] private MusicTrack[] musicTracks;

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
        MasterVolume = Mathf.Pow(volume, 2f);
        musicSource.volume = musicTracks[_currentClipIndex].volume * MasterVolume;
    }

    public void PlayClip(AudioClip clip, Vector3 position, float pitch = 1f)
    {
        GameObject obj = new GameObject($"AudioOneShot_{clip.name}");
        obj.transform.position = position;
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
            MusicTrack track = musicTracks[_currentClipIndex];
            musicSource.clip = track.clip;
            musicSource.volume = track.volume * MasterVolume;
            musicSource.Play();

            yield return new WaitForSeconds(track.clip.length);

            _currentClipIndex = (_currentClipIndex + 1) % musicTracks.Length;
        }
    }
}
