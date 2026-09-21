using System;
using System.Collections.Generic;
using UnityEngine;

// Author : Gazza Liu <gazzaliu@unizzagames.com>

/// <summary>Sfx 條目</summary>
[Serializable]
public class SfxDef
{
    [Tooltip("索引")] public string id;
    [Tooltip("音訊")] public AudioClip clip;

    [Tooltip("獨立覆寫冷卻時間, 小於等於0則使用全域預設值")]
    public float cooldownOverride;

    [Tooltip("音量係數, 疊在 AudioSource 音量與 Sfx mixer group 之上, 只能衰減")]
    [Range(0f, 1f)] public float volumeScale = 1f;

    // 防呆: 取得實際使用的音量
    public float ResolvedVolumeScale => volumeScale <= 0f ? 1f : volumeScale;
}

/// <summary>Bgm 條目</summary>
[Serializable]
public class BgmDef
{
    [Tooltip("索引")] public string id;
    [Tooltip("音訊")] public AudioClip clip;
    [Tooltip("名稱 (Loc Id)")] public string nameLocId;
    [Tooltip("是否循環播放")] public bool loop = true;
}

[CreateAssetMenu(fileName = "AudioDatabase", menuName = "Database/Audio Database")]
public class AudioDatabase : ScriptableObject
{
    [Tooltip("全域預設的音效防連播冷卻時間 (秒)")]
    [Min(0)] public float defaultSfxCooldown = 0.05f;

    public List<SfxDef> sfxList = new();
    public List<BgmDef> bgmList = new();

    public Dictionary<string, SfxDef> BuildSfxDict()
        => BuildDict(sfxList, e => e.id, e => e.clip != null, "Sfx");

    public Dictionary<string, BgmDef> BuildBgmDict()
        => BuildDict(bgmList, e => e.id, e => e.clip != null, "Bgm");

    /// <summary>以 id 建字典, 跳過空 id / 無 clip 的條目, 重複 id 警告並保留先出現的</summary>
    private Dictionary<string, T> BuildDict<T>(List<T> list, Func<T, string> idOf, Func<T, bool> hasClip, string label)
        where T : class
    {
        var dict = new Dictionary<string, T>();
        if (list == null) return dict;

        foreach (T entry in list)
        {
            if (entry == null) continue; // 防呆: 欄位增減後 Inspector 可能留下 null 元素

            string id = idOf(entry);
            if (string.IsNullOrWhiteSpace(id) || !hasClip(entry)) continue;

            if (!dict.ContainsKey(id))
                dict.Add(id, entry);
            else
                Debug.LogWarning($"[AudioDatabase] Duplicate {label} id: \"{id}\" in {name}");
        }
        return dict;
    }
}
