using UnityEngine;

[CreateAssetMenu(menuName = "Game/Tower SO", fileName = "NewTower")]
public class TowerSO : ScriptableObject
{
    [Header("Prefab")]
    public GameObject prefab;          // tower prefab with Tower component

    [Header("Cost")]
    public ItemSO costItem;            // e.g., Wood
    public int costAmount = 5;

    [Header("Combat")]
    public float range = 10f;
    public float fireRate = 1.0f;      // shots per second
    public int damage = 5;
    public float bulletSpeed = 18f;
    public GameObject projectilePrefab;
    //public AudioClip shootSFX;

}