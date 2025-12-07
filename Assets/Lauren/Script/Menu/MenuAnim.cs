using UnityEngine;
using UnityEngine.UI;

public class MenuAnim : MonoBehaviour
{
    public RectTransform obj;
    public RectTransform starAnchor;
    public float speed = 700f;
    public float cooldown = 5f;
    public float cooldownTimer = 5f; //change for start star time
    private Vector2 originalPos;

    void Start()
    {
        originalPos = obj.anchoredPosition;
    }

    void Update()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            obj.anchoredPosition = originalPos;
            return;
        }

        Vector2 pos = obj.anchoredPosition;
        pos = Vector2.MoveTowards(pos, starAnchor.anchoredPosition, speed * Time.deltaTime);
        obj.anchoredPosition = pos;

        if (Vector2.Distance(pos, starAnchor.anchoredPosition) < 1f)
        {
            // reached → reset + cooldown
            cooldownTimer = cooldown;
        }
    }
}