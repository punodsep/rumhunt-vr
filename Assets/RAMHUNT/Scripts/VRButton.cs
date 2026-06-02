using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class VRButton : MonoBehaviour
{
    public UnityEvent OnPressed;

    [Header("Sprites")]
    public Image buttonImage;
    public Sprite normalSprite;   // sprite ตั้งต้น
    public Sprite pressedSprite;  // sprite ตอนกด

    [Header("Settings")]
    public Transform rightHand;
    public Transform leftHand;
    public float activationDistance = 0.15f;
    public float resetDelay = 0.3f;  // กี่วินาทีก่อน reset กลับ

    float _cooldown;

    void Start()
    {
        if (buttonImage && normalSprite)
            buttonImage.sprite = normalSprite;
    }

    void Update()
    {
        if (_cooldown > 0f) { _cooldown -= Time.deltaTime; return; }

        if (rightHand != null) CheckDistance(rightHand.position);
        if (leftHand != null) CheckDistance(leftHand.position);
    }

    void CheckDistance(Vector3 handPos)
    {
        float dist = Vector3.Distance(handPos, transform.position);

        if (dist < activationDistance)
        {
            _cooldown = 1f;
            StartCoroutine(PressSequence());
        }
    }

    System.Collections.IEnumerator PressSequence()
    {
        // เปลี่ยนเป็น pressed sprite
        if (buttonImage && pressedSprite)
            buttonImage.sprite = pressedSprite;

        // รอนิดนึงให้เห็น feedback ก่อน
        yield return new WaitForSeconds(resetDelay);

        // reset กลับ normal
        if (buttonImage && normalSprite)
            buttonImage.sprite = normalSprite;

        // fire event หลัง reset
        OnPressed?.Invoke();
    }
}