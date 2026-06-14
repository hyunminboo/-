using UnityEngine;

[CreateAssetMenu(fileName = "NewMissionData", menuName = "Game/Mission Data")]
public class MissionData : ScriptableObject
{
    public string missionId = "MISSION 1";
    public string sceneName = "SampleScene";
    
    [TextArea(3, 10)]
    public string enemyDescription = "ENEMY INFO\n- Unknown Hostiles\n- Danger Level: HIGH";
}
