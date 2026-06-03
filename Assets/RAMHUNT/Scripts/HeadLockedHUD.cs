using UnityEngine;

public class HeadLockedHUD : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform headTransform;      // ลาก CenterEyeAnchor ใส่
    public float distance = 1.5f;
    public float heightOffset = 0.1f;
    public bool lockVerticalRotation = true;

    void LateUpdate()
    {
        if (headTransform == null) return;

        Vector3 forward = headTransform.forward;
        if (lockVerticalRotation)
        {
            forward.y = 0f;
            forward.Normalize();
        }

        // ติดทันทีไม่มี smooth
        transform.position = headTransform.position
                           + forward * distance
                           + Vector3.up * heightOffset;

        transform.rotation = Quaternion.LookRotation(
            transform.position - headTransform.position);
    }
}