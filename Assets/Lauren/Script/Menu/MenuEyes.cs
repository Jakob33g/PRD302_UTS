using UnityEngine;
using UnityEngine.UI;

public class MenuEyes : MonoBehaviour
{
    public Image img;
    public float blinkSpeed = 1f;
    float targetAlpha = 0f;

    void Update()
    {
        Color c = img.color;
        c.a = Mathf.MoveTowards(c.a, targetAlpha, blinkSpeed * Time.deltaTime);
        img.color = c;

        if (Mathf.Approximately(c.a, targetAlpha))
        {
            targetAlpha = (targetAlpha == 0f) ? 1f : 0f;
        }
    }
}