using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemySkill", menuName = "Enemy/ShootingSkill")]
public class AiDifficoulty : ScriptableObject
{
    [Range(0f, 1f)] public float accuracy = 0.8f;       // precisione
    [Range(0.5f, 1.5f)] public float powerMultiplier = 1f; // forza
    [Range(0f, 2f)] public float arcHeight = 1f;        // altezza extra
    public float reactionTime = 1f;
}
