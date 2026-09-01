using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource sfxSource;

    const string MusicVolKey = "MusicVolume";
    const string SfxVolKey = "SfxVolume";

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetMusicVolume(PlayerPrefs.GetFloat(MusicVolKey, 0.7f));
        SetSfxVolume(PlayerPrefs.GetFloat(SfxVolKey, 1f));
    }

    // Hook to a Settings panel Slider's OnValueChanged
    public void SetMusicVolume(float v)
    {
        if (musicSource != null) musicSource.volume = v;
        PlayerPrefs.SetFloat(MusicVolKey, v);
    }

    public void SetSfxVolume(float v)
    {
        if (sfxSource != null) sfxSource.volume = v;
        PlayerPrefs.SetFloat(SfxVolKey, v);
    }

    public void PlaySfx(AudioClip clip)
    {
        if (clip != null && sfxSource != null) sfxSource.PlayOneShot(clip);
    }
}