using UnityEngine;

[CreateAssetMenu(fileName = "DifficultyData", menuName = "SpoonTales/Difficulty Data")]
public class DifficultyData : ScriptableObject
{
    public string difficultyId = "Normal";

    [Header("套用到 RoundManager")]
    public float roundDuration = 60f;
    [Tooltip("乘在 Level01 基礎生成頻率曲線上的倍率")]
    public float spawnRateMultiplier = 1f;
    public float scoreMultiplier = 1f;

    [Header("套用到 Monster")]
    [Tooltip("乘在怪物每次移動所需時間上；小於 1 代表移動更快、更難閃避")]
    public float monsterMoveIntervalMultiplier = 1f;
}
