using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

// Author : Gazza Liu <gazzaliu@unizzagames.com>

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Database")]
    public AudioDatabase audioDatabase;

    [Header("Mixer Settings")]
    public AudioMixer mainMixer;
    public AudioMixerGroup musicGroup;
    public AudioMixerGroup sfxGroup;

    [Header("Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    public float MasterVolume { get; private set; }
    public float MusicVolume  { get; private set; }
    public float SoundVolume  { get; private set; }

    public static event Action<BgmDef> OnBgmChanged;

    private readonly Dictionary<string, SfxDef> _sfxDict = new();
    private readonly Dictionary<string, BgmDef> _bgmDict = new();
    private readonly Dictionary<string, float> _sfxNextPlayTime = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (musicSource) musicSource.outputAudioMixerGroup = musicGroup;
        if (sfxSource)   sfxSource.outputAudioMixerGroup   = sfxGroup;

        InitAudioDict();
    }

    void Start()
    {
        // 不可以放 Awake()有可能 mixer 無法接受參數而用預設值
        LoadVolume();
    }

    private void InitAudioDict()
    {
        if (audioDatabase == null)
        {
            Debug.LogError("[AudioManager] audioDatabase is not assigned!");
            return;
        }

        _sfxDict.Clear();
        foreach (var kv in audioDatabase.BuildSfxDict())
            _sfxDict.Add(kv.Key, kv.Value);

        _bgmDict.Clear();
        foreach (var kv in audioDatabase.BuildBgmDict())
            _bgmDict.Add(kv.Key, kv.Value);

        Debug.Log($"[AudioManager] Initialized with {_sfxDict.Count} Sfx / {_bgmDict.Count} Bgm entries from \"{audioDatabase.name}\".");
    }

    private const string PrefsKeyMasterVolume = "audio.master_volume";
    private const string PrefsKeyMusicVolume  = "audio.music_volume";
    private const string PrefsKeySoundVolume  = "audio.sound_volume";

    private void LoadVolume()
    {
        // 音量為 0~1，UI 以 0~100 顯示；存在 PlayerPrefs，重開遊戲後仍保留
        SetMasterVolume(PlayerPrefs.GetFloat(PrefsKeyMasterVolume, 1f));
        SetMusicVolume(PlayerPrefs.GetFloat(PrefsKeyMusicVolume, 1f));
        SetSoundVolume(PlayerPrefs.GetFloat(PrefsKeySoundVolume, 1f));
    }

    /// <summary>把目前音量寫入磁碟。拖曳 Slider 時只更新記憶體，放開/離開選單時再呼叫。</summary>
    public void SaveVolume()
    {
        PlayerPrefs.SetFloat(PrefsKeyMasterVolume, MasterVolume);
        PlayerPrefs.SetFloat(PrefsKeyMusicVolume, MusicVolume);
        PlayerPrefs.SetFloat(PrefsKeySoundVolume, SoundVolume);
        PlayerPrefs.Save();
    }

#region API - Volume
    public void SetMasterVolume(float volume)
    {
        MasterVolume = Mathf.Clamp01(volume);
        mainMixer.SetFloat("MasterVol", ToDecibel(MasterVolume));
    }

    public void SetMusicVolume(float volume)
    {
        MusicVolume = Mathf.Clamp01(volume);
        mainMixer.SetFloat("MusicVol", ToDecibel(MusicVolume));
    }

    public void SetSoundVolume(float volume)
    {
        SoundVolume = Mathf.Clamp01(volume);
        mainMixer.SetFloat("SfxVol", ToDecibel(SoundVolume));
    }

    private static float ToDecibel(float volume)
    {
        return volume <= 0.001f ? -80f : Mathf.Log10(volume) * 20f;
    }
#endregion

#region API - Bgm
    /// <summary>目前播放中 Bgm 的曲名 localization id (供 UI 顯示)</summary>
    public string CurrentBgmNameLocId { get; private set; }

    /// <summary>目前播放中 Bgm 的 id, 未播放時為 null</summary>
    public string CurrentBgmId { get; private set; }

    /// <summary>以 id 查詢 Bgm 條目, 給預載與外部查詢用</summary>
    public bool TryGetBgm(string id, out BgmDef def)
    {
        def = null;
        if (string.IsNullOrWhiteSpace(id)) return false;
        return _bgmDict.TryGetValue(id, out def);
    }

    /// <summary>預先載入 Bgm 的音訊資料, 避免播放當下的 I/O 卡頓</summary>
    public void PreloadBgm(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return;

        if (!TryGetBgm(id, out BgmDef def))
        {
            Debug.LogWarning($"[AudioManager] Bgm id not found, cannot preload: {id}");
            return;
        }

        if (def.clip != null) def.clip.LoadAudioData();
    }

    public void PlayMusic(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return;
        if (CurrentBgmId == id) return; // 同一首不重播, 也不重覆觸發事件

        if (!_bgmDict.TryGetValue(id, out BgmDef def))
        {
            Debug.LogWarning($"[AudioManager] Bgm id not found: {id}");
            return;
        }

        CurrentBgmId = id;
        CurrentBgmNameLocId = def.nameLocId;
        if (musicSource != null) musicSource.loop = def.loop;
        PlayMusicInternal(def.clip);

        OnBgmChanged?.Invoke(def);
    }

    public void StopMusic()
    {
        if (musicSource == null) return;
        musicSource.Stop();
        musicSource.clip = null;

        CurrentBgmId = null;
        CurrentBgmNameLocId = null;

        OnBgmChanged?.Invoke(null);
    }

    private void PlayMusicInternal(AudioClip clip)
    {
        if (musicSource == null) return;
        if (musicSource.clip == clip) return;
        musicSource.clip = clip;
        musicSource.Play();
    }

#endregion

#region API - Sfx
    public void PlaySfx(string id)
    {
        if (sfxSource == null || string.IsNullOrWhiteSpace(id)) return;

        if (!_sfxDict.TryGetValue(id, out SfxDef entry))
        {
            Debug.LogWarning($"[AudioManager] Sfx ID not found: {id}");
            return;
        }

        // 檢查冷卻時間
        if (_sfxNextPlayTime.TryGetValue(id, out float nextTime) && Time.unscaledTime < nextTime)
            return;

        // 播放音效
        PlaySfx(entry.clip, entry.ResolvedVolumeScale);

        // 更新下次可播放時間
        float cooldown = entry.cooldownOverride > 0f
            ? entry.cooldownOverride
            : audioDatabase.defaultSfxCooldown;

        _sfxNextPlayTime[id] = Time.unscaledTime + cooldown;
    }

    public void PlaySfx(string id, float delay)
    {
        if (delay <= 0f)
        {
            PlaySfx(id);
            return;
        }
        if (sfxSource == null || string.IsNullOrWhiteSpace(id)) return;

        StartCoroutine(PlaySfxDelayedRoutine(id, delay));
    }

    private IEnumerator PlaySfxDelayedRoutine(string id, float delay)
    {
        yield return new WaitForSeconds(delay);
        PlaySfx(id);
    }

    public void PlaySfx(AudioClip clip, float volumeScale = 1f)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip, Mathf.Clamp01(volumeScale));
    }
#endregion
}
