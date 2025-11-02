using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    Transform target;
    int damage;
    float speed;
    float life = 5f;

    Rigidbody rb;
    Collider col;

    public void Init(Transform target, int damage, float speed)
    {
        this.target = target;
        this.damage = damage;
        this.speed  = speed;

        if (!rb) rb = GetComponent<Rigidbody>();
        if (!col) col = GetComponent<Collider>();

        rb.isKinematic = true;
        rb.useGravity  = false;
        col.isTrigger = true;

    }

    void Update()
    {
        life -= Time.deltaTime;
        if (life <= 0f) { Destroy(gameObject); return; }

        if (target == null || !target.gameObject.activeInHierarchy)
        {
            // Fly straight if target gone
            transform.position += transform.forward * speed * Time.deltaTime;
            return;
        }

        // Home towards target
        Vector3 to = target.position - transform.position;
        float dist = to.magnitude;
        if (dist < 0.25f)
        {
            HitTarget();
            return;
        }
        Vector3 dir = to / Mathf.Max(0.0001f, dist);
        transform.rotation = Quaternion.LookRotation(dir);
        transform.position += dir * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        var e = other.GetComponentInParent<EnemyTest>();
        if (e != null)
        {
            e.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
    void HitTarget()
    {
        var e = target.GetComponentInParent<EnemyTest>();
        if (e != null) e.TakeDamage(damage);
        Destroy(gameObject);
    }
}