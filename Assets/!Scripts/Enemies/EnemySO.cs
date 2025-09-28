using UnityEngine;

[CreateAssetMenu(menuName = "Game/Enemy SO", fileName = "NewEnemy")]
public class EnemySO : ScriptableObject
{
    [Header("Identity")]
    public string id = "enemy_basic";
    public string displayName = "Grue";

    [Header("Prefab")]
    // A prefab with the Enemy component on the root
    public GameObject prefab;

    [Header("Stats")]
    [Min(1)]  public int   maxHealth    = 10;
    [Min(0)]  public float moveSpeed    = 2.5f;
    [Min(0)]  public float stopDistance = 1.2f;
    [Min(0)]  public float scale        = 1.0f;

    [Header("Rewards")]
    [Min(0)]  public int xpReward = 5;

    [Header("Spawning")]
    [Min(0f)] public float weight = 1f;
}