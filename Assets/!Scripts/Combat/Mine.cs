using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Mine : MonoBehaviour
{
    [Header("Mine Settings")]
    [Tooltip("Damage dealt to enemies in explosion radius")]
    public int damage = 100;
    [Tooltip("How far the explosion reaches")]
    public float explosionRadius = 3f;
    [Tooltip("Which layers count as enemies (leave at -1 for all layers)")]
    public LayerMask enemyLayer = -1;

    [Header("Visual - 3D Object")]
    [Tooltip("Drag a 3D object prefab here to show what the mine looks like (e.g., a sphere, cube, or custom model)")]
    public GameObject mine3DObject;
    
    [Header("Visual - Effects")]
    [Tooltip("Optional explosion effect prefab")]
    public GameObject explosionEffect;
    
    [Header("Visual - Legacy (deprecated)")]
    [Tooltip("Old field - use mine3DObject instead")]
    public GameObject mineVisual;

    Collider triggerCollider;
    bool hasExploded = false;
    
    [Header("Testing")]
    [Tooltip("If true, mine will explode when ANY collider touches it (for testing)")]
    public bool explodeOnAnyCollision = false;

    void Awake()
    {
        triggerCollider = GetComponent<Collider>();
        if (triggerCollider == null)
        {
            Debug.LogError("[Mine] No collider found! Adding SphereCollider...");
            SphereCollider sphereCol = gameObject.AddComponent<SphereCollider>();
            sphereCol.isTrigger = true;
            sphereCol.radius = 0.5f;
            triggerCollider = sphereCol;
        }
        
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
            
            // Make sure the collider is big enough
            if (triggerCollider is SphereCollider sphere)
            {
                if (sphere.radius < 0.5f)
                {
                    sphere.radius = 0.5f;
                    Debug.Log("[Mine] Increased trigger radius to 0.5");
                }
            }
            else if (triggerCollider is BoxCollider box)
            {
                if (box.size.magnitude < 1f)
                {
                    box.size = Vector3.one;
                    Debug.Log("[Mine] Increased trigger size to 1x1x1");
                }
            }
            
            Debug.Log($"[Mine] Trigger collider set up. Type: {triggerCollider.GetType().Name}, IsTrigger: {triggerCollider.isTrigger}, Enabled: {triggerCollider.enabled}");
        }

        // Use mine3DObject if assigned, otherwise fall back to mineVisual, otherwise create default
        if (mine3DObject != null)
        {
            // Instantiate the 3D object prefab
            GameObject visual = Instantiate(mine3DObject, transform);
            visual.name = "MineVisual";
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            
            // Remove any colliders from the visual (we only need the trigger on parent)
            Collider[] visualColliders = visual.GetComponentsInChildren<Collider>();
            foreach (Collider col in visualColliders)
            {
                Destroy(col);
            }
            
            mineVisual = visual;
            Debug.Log("[Mine] Instantiated 3D object from prefab");
        }
        else if (mineVisual == null)
        {
            CreateDefaultVisual();
        }
    }

    void CreateDefaultVisual()
    {
        // Create a child GameObject with a sphere mesh to represent the mine
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        visual.name = "MineVisual";
        visual.transform.SetParent(transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = Vector3.one * 0.3f; // Small sphere
        
        // Remove the collider from the visual (we only need the trigger collider on the parent)
        Collider visualCollider = visual.GetComponent<Collider>();
        if (visualCollider != null)
        {
            Destroy(visualCollider);
        }
        
        // Make it red/dark to look like a mine
        Renderer renderer = visual.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.3f, 0.1f, 0.1f); // Dark red
            renderer.material = mat;
        }
        
        mineVisual = visual;
        Debug.Log("[Mine] Created default 3D visual (sphere) for mine");
    }

    void Start()
    {
        // Verify everything is set up correctly
        Debug.Log($"[Mine] Start() - Mine at {transform.position}");
        Debug.Log($"[Mine] Has exploded: {hasExploded}");
        Debug.Log($"[Mine] Collider: {(triggerCollider != null ? "OK" : "MISSING")}");
        if (triggerCollider != null)
        {
            Debug.Log($"[Mine] Collider enabled: {triggerCollider.enabled}, IsTrigger: {triggerCollider.isTrigger}");
        }
        Debug.Log($"[Mine] Damage: {damage}, Radius: {explosionRadius}");
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasExploded) return;

        Debug.Log($"[Mine] ===== OnTriggerEnter CALLED! =====");
        Debug.Log($"[Mine] Collider: {other.name}, Tag: {other.tag}, Layer: {other.gameObject.layer}");
        Debug.Log($"[Mine] Mine position: {transform.position}, Other position: {other.transform.position}");
        Debug.Log($"[Mine] Mine collider: {(triggerCollider != null ? triggerCollider.name : "NULL")}, Enabled: {(triggerCollider != null ? triggerCollider.enabled.ToString() : "N/A")}");

        // Test mode: explode on any collision
        if (explodeOnAnyCollision)
        {
            Debug.Log($"[Mine] TEST MODE: Exploding on any collision!");
            Explode();
            return;
        }

        // Check if the collider belongs to an enemy
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy == null)
        {
            // Try to find enemy in parent
            enemy = other.GetComponentInParent<Enemy>();
        }
        
        // Also check if it's the enemy's collider (capsule/box collider)
        if (enemy == null)
        {
            // Check if the collider's GameObject has Enemy component
            enemy = other.gameObject.GetComponent<Enemy>();
        }
        
        // Check all children too
        if (enemy == null)
        {
            Enemy[] children = other.GetComponentsInChildren<Enemy>();
            if (children != null && children.Length > 0)
            {
                enemy = children[0];
            }
        }
        
        if (enemy != null)
        {
            Debug.Log($"[Mine] ✓✓✓ ENEMY '{enemy.name}' DETECTED! ✓✓✓");
            Debug.Log($"[Mine] Calling Explode() now...");
            Explode();
        }
        else
        {
            Debug.LogWarning($"[Mine] ✗ Collider '{other.name}' entered trigger but NO Enemy component found!");
            Debug.LogWarning($"[Mine] This might be the player or another object. Checking components...");
            
            // List all components on the collider's GameObject for debugging
            Component[] components = other.GetComponents<Component>();
            foreach (Component comp in components)
            {
                Debug.Log($"[Mine]   - Component: {comp.GetType().Name}");
            }
            
            // If it's the player, don't explode (unless in test mode)
            if (other.CompareTag("Player"))
            {
                Debug.Log("[Mine] Player touched mine - not exploding (mines only explode for enemies)");
            }
        }
    }
    
    void OnTriggerStay(Collider other)
    {
        // Also check OnTriggerStay in case OnTriggerEnter was missed
        if (hasExploded) return;
        
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy == null)
        {
            enemy = other.GetComponentInParent<Enemy>();
        }
        if (enemy == null)
        {
            enemy = other.gameObject.GetComponent<Enemy>();
        }
        
        if (enemy != null)
        {
            Debug.Log($"[Mine] Enemy detected in OnTriggerStay! Exploding...");
            Explode();
        }
    }

    void Explode()
    {
        if (hasExploded)
        {
            Debug.LogWarning("[Mine] Explode() called but mine already exploded!");
            return;
        }
        
        hasExploded = true;
        
        Debug.Log($"[Mine] ========================================");
        Debug.Log($"[Mine] BOOM! MINE EXPLODING!");
        Debug.Log($"[Mine] Position: {transform.position}");
        Debug.Log($"[Mine] Radius: {explosionRadius}, Damage: {damage}");
        Debug.Log($"[Mine] ========================================");

        // Find all enemies in explosion radius - use all colliders, then filter by Enemy component
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        
        int enemiesHit = 0;
        foreach (Collider hit in hits)
        {
            // Try multiple ways to find the Enemy component
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy == null)
            {
                enemy = hit.GetComponentInParent<Enemy>();
            }
            if (enemy == null)
            {
                enemy = hit.gameObject.GetComponent<Enemy>();
            }
            
            if (enemy != null)
            {
                // Check distance to make sure enemy is actually in range
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance <= explosionRadius)
                {
                    Debug.Log($"[Mine] Hitting enemy '{enemy.name}' at distance {distance:F2}m with {damage} damage");
                    enemy.TakeDamage(damage);
                    enemiesHit++;
                }
            }
        }

        // Also find enemies by searching all Enemy objects in scene (backup method)
        if (enemiesHit == 0)
        {
            Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
            foreach (Enemy enemy in allEnemies)
            {
                if (enemy == null || !enemy.gameObject.activeInHierarchy) continue;
                
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance <= explosionRadius)
                {
                    Debug.Log($"[Mine] Found enemy '{enemy.name}' via FindObjectsByType at distance {distance:F2}m");
                    enemy.TakeDamage(damage);
                    enemiesHit++;
                }
            }
        }

        Debug.Log($"[Mine] Explosion hit {enemiesHit} enemy/enemies");

        // Show explosion effect
        if (explosionEffect != null)
        {
            GameObject effect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }
        else
        {
            // Create a simple explosion visual if none is assigned
            CreateSimpleExplosionEffect();
        }

        // Hide mine visual immediately
        if (mineVisual != null)
        {
            mineVisual.SetActive(false);
            Debug.Log("[Mine] Mine visual hidden");
        }

        // Make the mine disappear immediately - disable ALL renderers
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        foreach (Renderer r in renderers)
        {
            if (r != null)
            {
                r.enabled = false;
                Debug.Log($"[Mine] Disabled renderer: {r.name}");
            }
        }
        
        // Disable all colliders
        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        foreach (Collider c in colliders)
        {
            if (c != null)
            {
                c.enabled = false;
                Debug.Log($"[Mine] Disabled collider: {c.name}");
            }
        }
        
        // Make the GameObject invisible
        gameObject.SetActive(false);
        Debug.Log("[Mine] GameObject deactivated");

        Debug.Log("[Mine] Destroying mine GameObject now...");
        
        // Destroy mine immediately (don't wait)
        Destroy(gameObject);
        
        Debug.Log("[Mine] Destroy() called - mine should be gone now");
    }

    void CreateSimpleExplosionEffect()
    {
        // Create a simple explosion effect using a sphere that scales up
        GameObject explosion = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        explosion.name = "ExplosionEffect";
        explosion.transform.position = transform.position;
        explosion.transform.localScale = Vector3.zero;
        
        // Make it orange/red
        Renderer renderer = explosion.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(1f, 0.5f, 0f); // Orange
            renderer.material = mat;
        }
        
        // Remove collider
        Collider col = explosion.GetComponent<Collider>();
        if (col != null)
            Destroy(col);
        
        // Animate it scaling up and fading out
        StartCoroutine(AnimateExplosion(explosion));
    }

    System.Collections.IEnumerator AnimateExplosion(GameObject explosion)
    {
        float duration = 0.3f;
        float elapsed = 0f;
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one * explosionRadius * 2f;
        
        Renderer renderer = explosion.GetComponent<Renderer>();
        Color startColor = renderer != null ? renderer.material.color : Color.white;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            explosion.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            
            if (renderer != null)
            {
                renderer.material.color = Color.Lerp(startColor, endColor, t);
            }
            
            yield return null;
        }
        
        Destroy(explosion);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}

