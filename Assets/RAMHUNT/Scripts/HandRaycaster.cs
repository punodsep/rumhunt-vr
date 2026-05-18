using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HandRaycaster : MonoBehaviour
{
    [Header("Hand Bones")]
    public OVRSkeleton leftSkeleton;
    public OVRSkeleton rightSkeleton;

    [Header("Settings")]
    public float pinchThreshold = 0.7f;  // ความแน่นของการหยิก
    public LayerMask uiLayer;

    OVRHand _leftHand, _rightHand;
    bool _wasPinching;

    void Start()
    {
        _leftHand = leftSkeleton.GetComponent<OVRHand>();
        _rightHand = rightSkeleton.GetComponent<OVRHand>();
    }

    void Update()
    {
        // ใช้มือขวาเป็นหลัก (เปลี่ยนเป็น left ได้)
        TryInteract(_rightHand, rightSkeleton);
    }

    void TryInteract(OVRHand hand, OVRSkeleton skeleton)
    {
        if (!hand.IsTracked) return;

        // ตรวจ Pinch (นิ้วโป้ง + นิ้วชี้หยิก)
        float pinchStrength = hand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
        bool isPinching = pinchStrength > pinchThreshold;

        // Raycast จากนิ้วชี้
        Transform indexTip = GetBoneTransform(skeleton, OVRSkeleton.BoneId.Hand_IndexTip);
        if (indexTip == null) return;

        Ray ray = new Ray(indexTip.position, indexTip.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 0.2f, uiLayer))
        {
            // กด (pinch เพิ่งเริ่ม)
            if (isPinching && !_wasPinching)
            {
                // ยิง Pointer Event ไปที่ UI
                var button = hit.collider.GetComponent<Button>();
                button?.onClick.Invoke();
            }
        }

        _wasPinching = isPinching;
    }

    Transform GetBoneTransform(OVRSkeleton sk, OVRSkeleton.BoneId id)
    {
        foreach (var b in sk.Bones)
            if (b.Id == id) return b.Transform;
        return null;
    }
}