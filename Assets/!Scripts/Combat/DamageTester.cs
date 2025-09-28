using UnityEngine;

public class DamageTester : MonoBehaviour
{
    public Health health; public float step = 10f;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K)) health.TakeDamage(step);
        if (Input.GetKeyDown(KeyCode.H)) health.Heal(step);
    }
}