using UnityEngine;

public class GemSpawner : MonoBehaviour
{
    public Transform[] spawnPoints;
    public ItemSO[] gemTypes;
    public int maxGems = 5;
    public float respawnTime = 10f;
    private int currentGems = 0;
    private bool[] occupied;

    void Start()
    {
        occupied = new bool[spawnPoints.Length];

        for (int i = 0; i < Mathf.Min(maxGems, spawnPoints.Length); i++)
        {
            SpawnGem();
        }
    }

    void SpawnGem()
    {
        if (currentGems >= maxGems) return;

        int[] freeIndices = GetFreeSpawnIndices();
        if (freeIndices.Length == 0) return;

        int spawnIndex = freeIndices[Random.Range(0, freeIndices.Length)];
        Transform spawn = spawnPoints[spawnIndex];
        ItemSO gemType = gemTypes[Random.Range(0, gemTypes.Length)];

        GameObject g = Instantiate(gemType.prefab, spawn.position, Quaternion.identity);

        // Optional: attach a callback for collection
        WorldPickup pickup = g.GetComponent<WorldPickup>();
        if (pickup != null) pickup.spawner = this;

        occupied[spawnIndex] = true;
        currentGems++;
    }


    int[] GetFreeSpawnIndices()
    {
        int freeCount = 0;
        for (int i = 0; i < occupied.Length; i++)
            if (!occupied[i]) freeCount++;

        int[] freeIndices = new int[freeCount];
        int idx = 0;
        for (int i = 0; i < occupied.Length; i++)
            if (!occupied[i]) freeIndices[idx++] = i;

        return freeIndices;
    }

    public void GemCollected(Transform gemTransform)
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i].position == gemTransform.position)
            {
                occupied[i] = false;
                break;
            }
        }

        currentGems--;
        Invoke(nameof(SpawnGem), respawnTime);
    }
}