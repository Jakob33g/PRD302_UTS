using UnityEngine;

public class PickupTest : MonoBehaviour
{
    public ItemSO item;
    public int amount = 2;
    public KeyCode pickupKey = KeyCode.E;
    public float bobAmplitude = 0.05f;
    public float bobSpeed = 3f;
    public bool autoPickup = false;
    public AudioClip pickupSFX;
    public AudioSource audioSource;
    bool inRange;
    ResourceTest inv;
    Vector3 basePos;

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
            inv = other.GetComponent<ResourceTest>();
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

        inv.AddResource(item, amount);

        if (pickupSFX)
            AudioSource.PlayClipAtPoint(pickupSFX, transform.position, 1f);

        Destroy(gameObject);
    }
}
