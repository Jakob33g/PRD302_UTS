using UnityEngine;

public class MineNew : MonoBehaviour
{
    public float fuseTime = 3f;
    public float flashSpeed = 0.5f;
    public float maxFlashSpeed = 10f;
    public float damage = 50f;
    public LayerMask damageMask;
    public GameObject bombExplosionPrefab;
    public SpriteRenderer spriteRenderer;
    public Transform damageArea;
    private Color originalColor;
    private bool isExploding = false;
    private float timer = 0f;

    void Awake()
    {
        if (!spriteRenderer)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer)
            originalColor = spriteRenderer.color;
    }

    void Update()
    {
        if (isExploding) return;

        timer += Time.deltaTime;
        float t = Mathf.PingPong(timer * flashSpeed, 1f);
        spriteRenderer.color = Color.Lerp(originalColor, Color.white, t);
        flashSpeed = Mathf.Lerp(flashSpeed, maxFlashSpeed, timer / fuseTime);

        if (timer >= fuseTime)
        {
            Explode();
        }
    }

    void Explode()
    {
        isExploding = true;

        if (bombExplosionPrefab)
            Instantiate(bombExplosionPrefab, transform.position, Quaternion.identity);

        if (damageArea)
        {
            Collider[] hits = Physics.OverlapSphere(damageArea.position, damageArea.localScale.x / 2f, damageMask);
            foreach (var hit in hits)
            {
                Health h = hit.GetComponent<Health>();
                if (h != null)
                    h.TakeDamage(damage);
            }
        }

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        if (damageArea != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(damageArea.position, damageArea.localScale.x / 2f);
        }
    }
}
