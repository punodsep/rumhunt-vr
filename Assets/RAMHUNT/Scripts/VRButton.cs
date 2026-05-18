// VRButton.cs — ใช้ Hand Anchor แทน Skeleton
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class VRButton : MonoBehaviour
{
    public UnityEvent OnPressed;
    public Image buttonImage;
    public Color normalColor = new Color(0.2f, 0.2f, 0.3f, 1f);
    public Color pressedColor = new Color(0.5f, 0.3f, 1f, 1f);

    [Header("Hand Reference")]
    public Transform rightHand;
    public Transform leftHand;

    [Header("Settings")]
    public float activationDistance = 0.1f;
    float _cooldown;

    void Update()
    {
        if (_cooldown > 0f) { _cooldown -= Time.deltaTime; return; }

        if (rightHand != null) CheckDistance(rightHand.position);
        if (leftHand != null) CheckDistance(leftHand.position);
    }

    void CheckDistance(Vector3 handPos)
    {
        float dist = Vector3.Distance(handPos, transform.position);
        Debug.Log($"[VRButton] dist: {dist:F3}");

        if (dist < activationDistance)
        {
            _cooldown = 1f;
            OnPressed?.Invoke();
            Debug.Log("[VRButton] PRESSED!");

            if (buttonImage != null)
            {
                buttonImage.color = pressedColor;
                Invoke(nameof(ResetColor), 0.3f);
            }
        }
    }

    void ResetColor()
    {
        if (buttonImage != null) buttonImage.color = normalColor;
    }
}