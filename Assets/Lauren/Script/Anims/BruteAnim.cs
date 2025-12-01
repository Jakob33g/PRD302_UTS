using UnityEngine;
using System.Collections;

public class BruteAnim : MonoBehaviour
{
    public Transform visualRoot;
    public Animator anim;
    public SpriteRenderer sr;
    public bool isAttacking = false;
    private bool isDying = false;
    private Color defaultColour;

    void Awake()
    {
        if (visualRoot != null)
        {
            anim = visualRoot.GetComponent<Animator>();
            sr = visualRoot.GetComponent<SpriteRenderer>();
            defaultColour = sr != null ? sr.color : Color.white;
        }
    }

    void Update()
    {
        if (isDying) return;
        if(anim != null) anim.SetBool("isAttacking", isAttacking);
    }
    public void PerformAttack(Transform target)
    {
        if (isDying || target == null) return;
        isAttacking = true;
    }

    public void StopAttack()
    {
        isAttacking = false;
    }

    public void TakeDamage(int dmg)
    {
        if (isDying) return;
        FlashRed();
    }

    public void Die()
    {
        if (isDying) return;
        isDying = true;

        if (anim != null)
            anim.SetTrigger("Die");

        StartCoroutine(FadeAndDespawn());
    }

    private void FlashRed()
    {
        if (sr == null) return;
        sr.color = Color.red;
        CancelInvoke(nameof(ResetColor));
        Invoke(nameof(ResetColor), 0.1f);
    }

    private void ResetColor()
    {
        if (sr != null)
            sr.color = defaultColour;
    }

    private IEnumerator FadeAndDespawn()
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime;
            if (sr != null)
            {
                Color c = sr.color;
                c.a = Mathf.Lerp(1f, 0f, t);
                sr.color = c;
            }
            yield return null;
        }

        gameObject.SetActive(false);
    }
}
