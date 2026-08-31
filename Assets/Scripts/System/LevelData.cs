using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "SpoonTales/Level Data")]
public class LevelData : ScriptableObject
{
    public string levelId = "Level01";
    public string levelName = "Monster Shoot";
    public string sceneName = "Level01_MonsterShoot";
    [TextArea] public string description = "依照球的顏色與正確時機消滅靠近的怪物。";
    public bool isUnlocked = true;
}
