using UnityEngine;

public class WorldPickup : MonoBehaviour
{
    [Header("What to give the player")]
    public ItemSO item;       // assign your Wood ItemSO here in Inspector
    public int amount = 1;    // how many to give on pickup

    [Header("Interaction")]
    public KeyCode pickupKey = KeyCode.E; // press E to pick up
    public float bobAmplitude = 0.05f;    // small idle float for visibility
    public float bobSpeed = 3f;
    public bool autoPickup = false;       // if true, picks up on enter (no key)

    public AudioClip pickupSFX;
    public AudioSource audioSource;

    bool inRange;
    Inventory inv;
    Vector3 basePos;

    ResourceTest invNew;

    void Start()
    {
        basePos = transform.position;

        if (!audioSource)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    void Update()
    {
        // Tiny float to make the item noticeable
        float y = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
        transform.position = basePos + new Vector3(0, y, 0);

        if (!inRange || inv == null) return;

        if (autoPickup || Input.GetKeyDown(pickupKey))
            TryPickup();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inRange = true;
            inv = other.GetComponent<Inventory>();
            invNew = other.GetComponent<ResourceTest>(); // lauren fix
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inRange = false;
            inv = null;
        }
    }

    void TryPickup()
    {
        if (inv == null || item == null || amount <= 0) return;

        invNew.AddResource(item, amount);

        int leftover = inv.AddItem(item, amount);

        if (pickupSFX && audioSource)
        {
            audioSource.PlayOneShot(pickupSFX);
        }

        if (leftover == 0)
        {
            // fully stored -> remove pickup
            Destroy(gameObject);
        }
        else
        {
            // Not all fit; keep the leftover amount in the world
            amount = leftover;
            Debug.Log("Inventory full; couldn't take all. Left on ground: " + leftover);
        }
    }

    // Simple debug prompt so you know it works (you can delete later)
    void OnGUI()
    {
        if (!inRange) return;
        Vector3 screen = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 0.5f);
        string text = autoPickup ? $"Picked up {item?.itemName}" : $"Press {pickupKey} to pick up {item?.itemName}";
        var size = new Vector2(200, 20);
        var rect = new Rect(screen.x - size.x/2, Screen.height - screen.y - size.y, size.x, size.y);
        GUI.Label(rect, text);
    }
}