using UnityEngine;

public class BombExplosion : MonoBehaviour
{
    private void Start()
    {
        Animator anim = GetComponent<Animator>();
        Destroy(gameObject, anim.GetCurrentAnimatorStateInfo(0).length);
    }
}